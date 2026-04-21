using KoenZomers.Tado.Api.Controllers;
using KoenZomers.Tado.Api.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;

public interface IDataRetrievalService
{
    Task RetrieveDataAsync(CancellationToken cancellationToken);
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

    public async Task RetrieveDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data retrieval at: {time}", DateTimeOffset.Now);

        try
        {
            var activeSchedule = await _dbContext.TadoRetrievalSchedules.Where(s => s.IsActive).FirstOrDefaultAsync();
            if (activeSchedule == null)
            {
                _logger.LogInformation("No active retrieval schedule found. Skipping data retrieval.");
                return;
            }

            if (activeSchedule.NextRetrievalTime > DateTimeOffset.Now)
            {
                _logger.LogInformation("Next retrieval time is in the future ({time}). Skipping data retrieval.", activeSchedule.NextRetrievalTime);
                return;
            }

            var token = await _dbContext.TadoTokens.FirstOrDefaultAsync(t => t.TokenId == activeSchedule.TokenId);
            if (token == null)            {
                _logger.LogWarning("Token with ID {tokenId} not found. Skipping data retrieval.", activeSchedule.TokenId);
                return;
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
                _logger.LogWarning("TODO: Refresh token logic not implemented. Failed to authenticate with Tado using token ID {tokenId}. Skipping data retrieval.", activeSchedule.TokenId);
                return;
            }

        var me = await _tadoService.GetMe();

        if (me == null || me.Homes == null || me.Homes.Length == 0)
        {
            //probably this is where I need that refresh token...
            _logger.LogWarning("No homes found for the authenticated user.");

            tadoToken = await _tadoService.GetAccessTokenWithRefreshToken(tadoToken.RefreshToken, cancellationToken);

            if (tadoToken == null)
            {
                _logger.LogWarning("Failed to refresh token. Skipping data retrieval.");
                return;
            }
            if (!_tadoService.Authenticate(tadoToken))
            {
                _logger.LogWarning("Despite the refresh token being obtained, failed to authenticate with Tado using token ID {tokenId}. Skipping data retrieval.", activeSchedule.TokenId);
                return;
            }
            me = await _tadoService.GetMe();
            if (me == null || me.Homes == null || me.Homes.Length == 0)
            {
                _logger.LogWarning("Even after refreshing the token, no homes found for the authenticated user. Skipping data retrieval.");
                return;
            }

            token.AccessToken = $"{tadoToken.AccessToken}";
            token.RefreshToken = $"{tadoToken.RefreshToken}";
            token.ExpiresIn = tadoToken.ExpiresIn??0;
            token.Scope = $"{tadoToken.Scope}";
            token.TokenType = $"{tadoToken.TokenType}";
            token.UserId = $"{tadoToken.UserId}";
            _dbContext.TadoTokens.Update(token);
            await _dbContext.SaveChangesAsync();
        }

        var homeId = Convert.ToInt32(me.Homes.Single().Id??0);
        var zones = await _tadoService.GetZones(homeId);

        if(zones == null || zones.Length == 0)
        {
            _logger.LogWarning("No zones found for the home.");
            return;
        }

        var zoneName = string.Empty;
        var insiteTemperature = (double) 0;
        var humidityPercentage = (double) 0;

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


            _logger.LogInformation("Data retrieval completed successfully at: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during data retrieval.");
        }
    }
}