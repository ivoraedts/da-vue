using KoenZomers.Tado.Api.Controllers;
using KoenZomers.Tado.Api.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWeb.API.Exceptions;

public interface IDataRetrievalService
{
    Task<DateTime> RetrieveDataAsync(CancellationToken cancellationToken);
}

public class DataRetrievalService : IDataRetrievalService
{
    private readonly ILogger<DataRetrievalService> _logger;
    private readonly Tado _tadoService;
    private readonly DaVueDbContext _dbContext;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving data.");
            return DateTime.UtcNow.AddMinutes(5); // In case something goes wrong, try again in 5 minutes
        }
    }

    private async Task<DateTime> TryRetrieveDataOrThrowException(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data retrieval at: {time}", DateTimeOffset.Now);

        var activeSchedule = await _dbContext.TadoRetrievalSchedules.Where(s => s.IsActive).FirstOrDefaultAsync();
        if (activeSchedule == null)
        {
            throw new Exception("No active retrieval schedule found.");
        }

        if (activeSchedule.NextRetrievalTime > DateTimeOffset.Now)
        {
            throw new ToEarlyException(activeSchedule.NextRetrievalTime, $"Next retrieval time is in the future ({activeSchedule.NextRetrievalTime:O}). Skipping data retrieval.");
        }

        var token = await _dbContext.TadoTokens.FirstOrDefaultAsync(t => t.TokenId == activeSchedule.TokenId);
        if (token == null)
        {
            throw new Exception($"No token found for token ID {activeSchedule.TokenId}.");
        }

        var tadoToken = new Token
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresIn = token.ExpiresIn,
            Scope = token.Scope,
            TokenType = token.TokenType,
            UserId = token.UserId
        };

        if (!_tadoService.Authenticate(tadoToken))
        {
            throw new Exception($"Something is wrong with the token related to token {activeSchedule.TokenId}. Even after expiring, this is not the point where things should go wrong. Skipping data retrieval.");
        }

        var me = await _tadoService.GetMe();

        if (me == null || me.Homes == null || me.Homes.Length == 0)
        {
            _logger.LogWarning($"Token is expired and needs a refresh. Attempting to refresh the token for token ID {activeSchedule.TokenId}.");
            tadoToken = await _tadoService.GetAccessTokenWithRefreshToken(tadoToken.RefreshToken, cancellationToken);

            if (tadoToken == null)
            {
                throw new Exception("Failed to refresh token. Skipping data retrieval.");
            }
            if (!_tadoService.Authenticate(tadoToken))
            {
                throw new Exception($"Despite the refresh token being obtained, failed to authenticate with Tado using token ID {activeSchedule.TokenId}. Skipping data retrieval.");
            }
            me = await _tadoService.GetMe();
            if (me == null || me.Homes == null || me.Homes.Length == 0)
            {
                throw new Exception("Even after refreshing the token, no homes found for the authenticated user. Skipping data retrieval.");
            }

            token.AccessToken = $"{tadoToken.AccessToken}";
            token.RefreshToken = $"{tadoToken.RefreshToken}";
            token.ExpiresIn = tadoToken.ExpiresIn ?? 0;
            token.Scope = $"{tadoToken.Scope}";
            token.TokenType = $"{tadoToken.TokenType}";
            token.UserId = $"{tadoToken.UserId}";
            _dbContext.TadoTokens.Update(token);
            await _dbContext.SaveChangesAsync();
        }

        var homeId = Convert.ToInt32(me.Homes.Single().Id ?? 0);
        var zones = await _tadoService.GetZones(homeId);

        if (zones == null || zones.Length == 0)
        {
            throw new Exception("No zones found for the home.");
        }

        var zoneName = string.Empty;
        var insiteTemperature = (double)0;
        var humidityPercentage = (double)0;

        foreach (var zone in zones)
        {
            zoneName = zone.Name;
            var state = await _tadoService.GetZoneState(homeId, Convert.ToInt16(zone.Id));
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.InsideTemperature != null && state.SensorDataPoints.InsideTemperature.Celsius != null)
            {
                insiteTemperature = state.SensorDataPoints.InsideTemperature.Celsius.Value;
            }
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.Humidity != null && state.SensorDataPoints.Humidity.Percentage != null)
            {
                humidityPercentage = state.SensorDataPoints.Humidity.Percentage.Value;
            }

            if (insiteTemperature != 0 && humidityPercentage != 0)
            {
                break; // Exit the loop if we have valid temperature and humidity values
            }
        }

        var newRetrievedData = new TadoRetrievedData
        {
            ScheduleId = activeSchedule.ScheduleId,
            HomeId = homeId,
            ZoneName = $"{zoneName}",
            InsideTemperatureCelsius = insiteTemperature,
            HumidityPercentage = humidityPercentage,
            RetrievedAt = DateTime.UtcNow
        };

        _dbContext.TadoRetrievedData.Add(newRetrievedData);
        await _dbContext.SaveChangesAsync();

        activeSchedule.NextRetrievalTime = activeSchedule.NextRetrievalTime.AddMinutes(activeSchedule.Interval);
        if (activeSchedule.NextRetrievalTime < DateTime.UtcNow)
        {
            //for some reason it was really late, so the calculated next retrieval time is already in the past. To avoid multiple quick retrievals, we set the next retrieval time to now + interval.
            activeSchedule.NextRetrievalTime = DateTime.UtcNow.AddMinutes(activeSchedule.Interval);
        }

        activeSchedule.LastRetrievalTime = DateTime.UtcNow;
        _dbContext.TadoRetrievalSchedules.Update(activeSchedule);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Data retrieval completed successfully at: {time}", DateTimeOffset.Now);
        return activeSchedule.NextRetrievalTime;
    }
}