using KoenZomers.Tado.Api.Controllers;
using KoenZomers.Tado.Api.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWeb.API.Exceptions;
using TheWeb.API.Extensions;
using TheWeb.API.Models;

namespace TheWeb.API.Services;

public interface IDataRetrievalService
{
    Task<DateTime> RetrieveDataAsync(CancellationToken cancellationToken);
}

public class DataRetrievalService : IDataRetrievalService
{
    private readonly ILogger<DataRetrievalService> _logger;
    private readonly Tado _tadoService;
    private readonly DaVueDbContext _dbContext;

    const int DelayInMinutesWithoutActiveSchedule = 5;
    const int DelayInMinutesAfterFailure = 5;

    public DataRetrievalService(ILogger<DataRetrievalService> logger, Tado tadoService, DaVueDbContext dbContext)
    {
        _logger = logger;
        _tadoService = tadoService;
        _dbContext = dbContext;
    }

    public async Task<DateTime> RetrieveDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await TryRetrieveDataOrThrowException(cancellationToken);
        }
        catch (ToEarlyException ex)
        {
            _logger.LogInformation($"ToEarlyException occurred: {ex.Message}. Next retrieval time is at: {ex.NextRetrievalTime:O}, so waiting for {ex.NextRetrievalTime - DateTime.UtcNow}.");
            return ex.NextRetrievalTime;
        }
        catch (NoActiveScheduleException)
        {
            _logger.LogWarning($"No active schedule found. Trying again in {DelayInMinutesWithoutActiveSchedule} few minutes. At {DateTime.UtcNow.AddMinutes(DelayInMinutesWithoutActiveSchedule):O}.");
            return DateTime.UtcNow.AddMinutes(DelayInMinutesWithoutActiveSchedule);
        }
        catch (TadoRequestThrottledException ex)
        {
            var delay = DelayInMinutesAfterFailure * ex.NumberOfConsecutiveFailures * ex.NumberOfConsecutiveFailures * 2;
            var nextRetrievalTime = DateTime.UtcNow.AddMinutes(delay);
            _logger.LogWarning($"TadoRequestThrottledException occurred. Number of consecutive failures: {ex.NumberOfConsecutiveFailures}. Trying again in {delay} minutes. At {nextRetrievalTime:O}.");
            return nextRetrievalTime;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while retrieving data: {ex}");
            return DateTime.UtcNow.AddMinutes(DelayInMinutesAfterFailure); // In case something goes wrong, try again in 5 minutes
        }
    }

    private async Task<DateTime> TryRetrieveDataOrThrowException(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data retrieval at: {time}", DateTimeOffset.Now);


        var schedule = await GetValidatedActiveSchedule();
        try
        {
            var userInfo = await AuthenticateAndRetrieveUserInfo(schedule.TokenId, cancellationToken);
            var retrievedData = await RetrieveBasicData(userInfo);

            await StoreRetrievedData(retrievedData, schedule.ScheduleId);
            await UpdateRetrievalSchedule(schedule);

            _logger.LogInformation("Data retrieval completed successfully at: {time}", DateTimeOffset.Now);
            return schedule.NextRetrievalTime;
        }
        catch (KoenZomers.Tado.Api.Exceptions.RequestThrottledException ex)
        {
            await RegisterFailureInSchedule(schedule, ex);
            _logger.LogError($"Request was throttled by the Tado API with error: {ex}");
            throw new TadoRequestThrottledException(schedule.ConsecutiveFailures, ex.Message);
        }
        catch (Exception ex)
        {
            await RegisterFailureInSchedule(schedule, ex);
            throw; // Rethrow the exception to be handled in the calling method and to avoid updating the next retrieval time, since the retrieval was not successful.
        }

    }

    private async Task<TadoRetrievalSchedule> GetValidatedActiveSchedule()
    {
        var activeSchedule = await _dbContext.TadoRetrievalSchedules.Where(s => s.IsActive).OrderBy(s => s.ScheduleId).FirstOrDefaultAsync();
        if (activeSchedule == null)
        {
            throw new NoActiveScheduleException();
        }

        if (activeSchedule.NextRetrievalTime > DateTimeOffset.Now)
        {
            throw new ToEarlyException(activeSchedule.NextRetrievalTime, $"Next retrieval time is in the future ({activeSchedule.NextRetrievalTime:O}). Skipping data retrieval.");
        }
        return activeSchedule;
    }

    private async Task<KoenZomers.Tado.Api.Models.User> AuthenticateAndRetrieveUserInfo(int tokenId, CancellationToken cancellationToken)
    {
        var token = await GetTokenFromDatabase(tokenId);
        var tadoToken = GetAuthenticatedToken(tokenId, token);
        return await GetUserInfo(tokenId, token, tadoToken, cancellationToken);
    }

    private async Task<TadoToken> GetTokenFromDatabase(int tokenId)
    {
        var token = await _dbContext.TadoTokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
        if (token == null)
        {
            throw new Exception($"No token found for token ID {tokenId}.");
        }
        return token;
    }

    private Token GetAuthenticatedToken(int tokenId, TadoToken token)
    {
        var tadoToken = token.ConvertToTadoToken();
        AuthenticateToken(tokenId, tadoToken);
        return tadoToken;
    }

    private void AuthenticateToken(int tokenId, Token tadoToken)
    {
        if (!_tadoService.Authenticate(tadoToken))
        {
            throw new Exception($"Something is wrong with the token related to token {tokenId}. Even after expiring, this is not the point where things should go wrong. Skipping data retrieval.");
        }
    }

    private async Task<KoenZomers.Tado.Api.Models.User> GetUserInfo(int tokenId, TadoToken token, Token tadoToken, CancellationToken cancellationToken)
    {
        var userInfo = await _tadoService.GetMe();

        if (userInfo == null || userInfo.Homes == null || userInfo.Homes.Length == 0)
        {
            return await RefreshTokenAndGetUserInfo(tokenId, token, tadoToken, cancellationToken);
        }

        return userInfo;
    }

    private async Task<KoenZomers.Tado.Api.Models.User> RefreshTokenAndGetUserInfo(int tokenId, TadoToken token, Token tadoToken, CancellationToken cancellationToken)
    {
        _logger.LogWarning($"Token is expired and needs a refresh. Attempting to refresh the token for token ID {tokenId}.");
        tadoToken = await RefreshTokenAndAuthenticateIt(tokenId, $"{tadoToken.RefreshToken}", cancellationToken);
        var userInfo = await _tadoService.GetMe();
        if (userInfo == null || userInfo.Homes == null || userInfo.Homes.Length == 0)
        {
            throw new Exception("Even after refreshing the token, no homes found for the authenticated user. Skipping data retrieval.");
        }

        await StoreRefreshedToken(token, tadoToken);
        return userInfo;
    }

    private async Task<Token> RefreshTokenAndAuthenticateIt(int tokenId, string refreshToken, CancellationToken cancellationToken)
    {
        var tadoToken = await _tadoService.GetAccessTokenWithRefreshToken(refreshToken, cancellationToken);

        if (tadoToken == null)
        {
            throw new Exception("Failed to refresh token. Skipping data retrieval.");
        }
        if (!_tadoService.Authenticate(tadoToken))
        {
            throw new Exception($"Despite the refresh token being obtained, failed to authenticate with Tado using token ID {tokenId}. Skipping data retrieval.");
        }

        return tadoToken;
    }

    private async Task StoreRefreshedToken(TadoToken token, Token tadoToken)
    {
        token.AccessToken = $"{tadoToken.AccessToken}";
        token.RefreshToken = $"{tadoToken.RefreshToken}";
        token.ExpiresIn = tadoToken.ExpiresIn ?? 0;
        token.Scope = $"{tadoToken.Scope}";
        token.TokenType = $"{tadoToken.TokenType}";
        token.UserId = $"{tadoToken.UserId}";
        _dbContext.TadoTokens.Update(token);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<SimpleTadoRetrievedData> RetrieveBasicData(KoenZomers.Tado.Api.Models.User userInfo)
    {
        int homeId = GetHomeId(userInfo);
        var zones = await GetZones(homeId);
        var result = new SimpleTadoRetrievedData
        {
            HomeId = homeId,
            ZoneName = string.Empty,
            InsideTemperatureCelsius = (double)0,
            HumidityPercentage = (double)0
        };

        foreach (var zone in zones)
        {
            result.ZoneName = $"{zone.Name}";
            var state = await _tadoService.GetZoneState(homeId, Convert.ToInt16(zone.Id));
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.InsideTemperature != null && state.SensorDataPoints.InsideTemperature.Celsius != null)
            {
                result.InsideTemperatureCelsius = state.SensorDataPoints.InsideTemperature.Celsius.Value;
            }
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.Humidity != null && state.SensorDataPoints.Humidity.Percentage != null)
            {
                result.HumidityPercentage = state.SensorDataPoints.Humidity.Percentage.Value;
            }

            if (result.InsideTemperatureCelsius != 0 && result.HumidityPercentage != 0)
            {
                break; // Exit the loop if we have valid temperature and humidity values
            }
        }
        return result;
    }

    private static int GetHomeId(KoenZomers.Tado.Api.Models.User userInfo)
    {
        if (userInfo.Homes == null || userInfo.Homes.Length == 0)
        {
            throw new Exception("No homes found for the authenticated user. Skipping data retrieval.");
        }

        if (userInfo.Homes.Length > 1)
        {
            throw new Exception("Multiple homes found for the authenticated user. This is currently not supported, so skipping data retrieval.");
        }
        return Convert.ToInt32(userInfo.Homes.Single().Id ?? 0);
    }

    private async Task<KoenZomers.Tado.Api.Models.Zone[]> GetZones(int homeId)
    {
        var zones = await _tadoService.GetZones(homeId);
        if (zones == null || zones.Length == 0)
        {
            throw new Exception("No zones found for the home.");
        }
        return zones;
    }

    private async Task StoreRetrievedData(SimpleTadoRetrievedData retrievedData, int scheduleId)
    {
        var newRetrievedData = new TadoRetrievedData
        {
            ScheduleId = scheduleId,
            HomeId = retrievedData.HomeId,
            ZoneName = retrievedData.ZoneName,
            InsideTemperatureCelsius = retrievedData.InsideTemperatureCelsius,
            HumidityPercentage = retrievedData.HumidityPercentage,
            RetrievedAt = DateTime.UtcNow
        };

        _dbContext.TadoRetrievedData.Add(newRetrievedData);
        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateRetrievalSchedule(TadoRetrievalSchedule schedule)
    {
        schedule.NextRetrievalTime = schedule.NextRetrievalTime.AddMinutes(schedule.Interval);
        if (schedule.NextRetrievalTime < DateTime.UtcNow)
        {
            //for some reason it was really late, so the calculated next retrieval time is already in the past. To avoid multiple quick retrievals, we set the next retrieval time to now + interval.
            schedule.NextRetrievalTime = DateTime.UtcNow.AddMinutes(schedule.Interval);
        }

        schedule.LastRetrievalTime = DateTime.UtcNow;
        schedule.ConsecutiveFailures = 0;
        _dbContext.TadoRetrievalSchedules.Update(schedule);
        await _dbContext.SaveChangesAsync();
    }

    private async Task RegisterFailureInSchedule(TadoRetrievalSchedule schedule, Exception ex)
    {
        schedule.ConsecutiveFailures += 1;
        if (schedule.ConsecutiveFailures >= 5)
        {
            schedule.LastError = ex.Message;
            _logger.LogError(ex, $"Data retrieval failed {schedule.ConsecutiveFailures} times in a row for schedule ID {schedule.ScheduleId}. Deactivating the schedule to prevent further issues.");
            schedule.IsActive = false;
        }
        _dbContext.TadoRetrievalSchedules.Update(schedule);
        await _dbContext.SaveChangesAsync();
    }
}