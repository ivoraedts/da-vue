using KoenZomers.Tado.Api.Controllers;
using KoenZomers.Tado.Api.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWebApi.Models;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/tadotemperature
public class TadoTemperatureController : ControllerBase
{
    private readonly Tado _tadoService;
    private readonly DaVueDbContext _dbContext;

    public TadoTemperatureController(Tado tado, DaVueDbContext dbContext)
    {
        _tadoService = tado;
        _dbContext = dbContext;
    }

    [Route("get-new-url")] // This makes the URL: api/tadotemperature/get-new-url
    [HttpGet]
    public async Task<ActionResult<TheWebApi.Models.TadoInitialization>> Get()
    {

        var deviceAuthorization = await _tadoService.GetDeviceCodeAuthentication(CancellationToken.None);
        if (deviceAuthorization==null)
        {
            return NotFound();
        }

        var newDeviceAuthorization = new TheWeb.API.Data.TadoDeviceAuthentication
        {
           Creation = DateTime.UtcNow,
           DeviceCode = $"{deviceAuthorization.DeviceCode}",
           ExpiresIn = (deviceAuthorization.ExpiresIn == null) ? (short) 0 : deviceAuthorization.ExpiresIn.Value,
           Interval = (deviceAuthorization.Interval == null) ? (short) 0 : deviceAuthorization.Interval.Value,
           UserCode = $"{deviceAuthorization.UserCode}",
           VerificationUri = $"{deviceAuthorization.VerificationUri}",
           VerificationUriComplete = $"{deviceAuthorization.VerificationUriComplete}",
        };
        _dbContext.TadoDeviceAuthentications.Add(newDeviceAuthorization);
        await _dbContext.SaveChangesAsync();
        
        return Ok(new TadoInitialization
        {
            CommunicationId = newDeviceAuthorization.CommunicationId,
            VerificationUriComplete = newDeviceAuthorization.VerificationUriComplete,
            UserCode = newDeviceAuthorization.UserCode
        }); // Returns a 200 OK status with the JSON object
    }

    [Route("authenticate/{communicationId}")] // This makes the URL: api/tadotemperature/authenticate/{communicationId}
    [HttpGet]
    public async Task<ActionResult<TheWebApi.Models.ActualTadoData>> GetByCommunicationId(int communicationId)
    {
        var storedDeviceAuthorization = await _dbContext.TadoDeviceAuthentications.FirstOrDefaultAsync(d => d.CommunicationId==communicationId);
        if (storedDeviceAuthorization == null)
        {
            return NotFound();
        }

        var dev_auth_resp = new DeviceAuthorizationResponse
        {
            DeviceCode = storedDeviceAuthorization.DeviceCode,
            ExpiresIn = storedDeviceAuthorization.ExpiresIn,
            Interval = storedDeviceAuthorization.Interval,
            UserCode = storedDeviceAuthorization.UserCode,
            VerificationUri = storedDeviceAuthorization.VerificationUri,
            VerificationUriComplete = storedDeviceAuthorization.VerificationUriComplete
        };

        var tokenResponse = await _tadoService.WaitForDeviceCodeAuthenticationToComplete(dev_auth_resp, CancellationToken.None);
        if (tokenResponse == null)
        {
            throw new InvalidOperationException($"Failed to wait for token response");
        }

        var newToken = new TadoToken
        {
            AccessToken = $"{tokenResponse.AccessToken}",
            RefreshToken = $"{tokenResponse.RefreshToken}",
            ExpiresIn = tokenResponse.ExpiresIn ?? 0,
            Scope = $"{tokenResponse.Scope}",
            TokenType = $"{tokenResponse.TokenType}",
            UserId = $"{tokenResponse.UserId}"
        };
        _dbContext.TadoTokens.Add(newToken);
        await _dbContext.SaveChangesAsync();

        if (!_tadoService.Authenticate(tokenResponse))
        {
            return Unauthorized();
        }
        var me = await _tadoService.GetMe();
        var homeId = (int)me.Homes.Single().Id;
        var zones = await _tadoService.GetZones(homeId);

        var zoneName = string.Empty;
        var insiteTemperature = (double) 0;
        var humidityPercentage = (double) 0;

        foreach (var zone in zones)
        {
            zoneName = zone.Name;
            var state = await _tadoService.GetZoneState(homeId, (short)zone.Id);
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.InsideTemperature != null)
            {
                insiteTemperature = state.SensorDataPoints.InsideTemperature.Celsius.Value;
            }
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.Humidity != null)
            {
                humidityPercentage = state.SensorDataPoints.Humidity.Percentage.Value;
            }

            if (insiteTemperature != 0 && humidityPercentage != 0)
            {
                break; // Exit the loop if we have valid temperature and humidity values
            }
        }

        return Ok(new ActualTadoData
        {
            HomeId = homeId,
            InsideTemperatureCelsius = insiteTemperature,
            HumidityPercentage = humidityPercentage,
            TokenId = newToken.TokenId,
            ZoneName = $"{zoneName}",
        }); // Returns a 200 OK status with the JSON object
    }

    [Route("addSchedule")] // This makes the URL: api/tadotemperature/addSchedule/{communicationId}
    [HttpPost]
    public async Task<ActionResult<CreatedTadoSchedule>> AddTrackingSchedule([FromBody] SetSchedule setSchedule)
    {
        var token = await _dbContext.TadoTokens.FirstOrDefaultAsync(t => t.TokenId == setSchedule.TokenId);
        if (token == null)
        {
            return NotFound();
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
            return Unauthorized();
        }

        var me = await _tadoService.GetMe();
        var homeId = (int)me.Homes.Single().Id;
        var zones = await _tadoService.GetZones(homeId);

        var zoneName = string.Empty;
        var insiteTemperature = (double) 0;
        var humidityPercentage = (double) 0;

        foreach (var zone in zones)
        {
            zoneName = zone.Name;
            var state = await _tadoService.GetZoneState(homeId, (short)zone.Id);
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.InsideTemperature != null)
            {
                insiteTemperature = state.SensorDataPoints.InsideTemperature.Celsius.Value;
            }
            if (state != null && state.SensorDataPoints != null && state.SensorDataPoints.Humidity != null)
            {
                humidityPercentage = state.SensorDataPoints.Humidity.Percentage.Value;
            }

            if (insiteTemperature != 0 && humidityPercentage != 0)
            {
                break; // Exit the loop if we have valid temperature and humidity values
            }
        }

        var newSchedule = new TadoRetrievalSchedule
        {
            TokenId = setSchedule.TokenId,
            Interval = setSchedule.IntervalInMinutes,
            NextRetrievalTime = DateTime.UtcNow.AddMinutes(setSchedule.IntervalInMinutes),
            LastRetrievalTime = DateTime.UtcNow,
            IsActive = true,
            HomeId = homeId,
            ZoneName = $"{zoneName}",
            LastError = string.Empty,
            ConsecutiveFailures = 0
        };

        _dbContext.TadoRetrievalSchedules.Add(newSchedule);
        await _dbContext.SaveChangesAsync();

        var newRetrievedData = new TadoRetrievedData
        {
            ScheduleId = newSchedule.ScheduleId,
            HomeId = homeId,
            ZoneName = $"{zoneName}",
            InsideTemperatureCelsius = insiteTemperature,
            HumidityPercentage = humidityPercentage,
            RetrievedAt = DateTime.UtcNow
        };

        _dbContext.TadoRetrievedData.Add(newRetrievedData);
        await _dbContext.SaveChangesAsync();

        return Ok(new CreatedTadoSchedule
        {
            ScheduleId = newSchedule.ScheduleId,
            HomeId = homeId,
            InsideTemperatureCelsius = insiteTemperature,
            HumidityPercentage = humidityPercentage,            
            ZoneName = $"{zoneName}",
            TokenId = newSchedule.TokenId,
            Interval = newSchedule.Interval,
            NextRetrievalTime = newSchedule.NextRetrievalTime,
            LastRetrievalTime = newSchedule.LastRetrievalTime
        }); // Returns a 200 OK status with the JSON object
    }
}