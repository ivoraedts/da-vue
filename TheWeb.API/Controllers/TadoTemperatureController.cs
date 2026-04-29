using KoenZomers.Tado.Api.Controllers;
using KoenZomers.Tado.Api.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWeb.API.Extensions;
using TheWeb.API.Models;

namespace TheWeb.API.Controllers;

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
    public async Task<ActionResult<TadoInitialization>> Get()
    {

        var deviceAuthorization = await _tadoService.GetDeviceCodeAuthentication(CancellationToken.None);
        if (deviceAuthorization == null)
        {
            return NotFound();
        }

        var newDeviceAuthorization = new TadoDeviceAuthentication
        {
            Creation = DateTime.UtcNow,
            DeviceCode = $"{deviceAuthorization.DeviceCode}",
            ExpiresIn = (deviceAuthorization.ExpiresIn == null) ? (short)0 : deviceAuthorization.ExpiresIn.Value,
            Interval = (deviceAuthorization.Interval == null) ? (short)0 : deviceAuthorization.Interval.Value,
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
    public async Task<ActionResult<ActualTadoData>> GetByCommunicationId(int communicationId)
    {
        var storedDeviceAuthorization = await _dbContext.TadoDeviceAuthentications.FirstOrDefaultAsync(d => d.CommunicationId == communicationId);
        if (storedDeviceAuthorization == null)
        {
            return NotFound("Device authorization not found");
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
            return NotFound("Token response not found. Authentication might not be completed yet.");
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
            return Unauthorized("Failed to authenticate with Tado.");
        }
        var me = await _tadoService.GetMe();
        if (me == null || me.Homes == null || me.Homes.Length == 0)
        {
            return NotFound("No homes found for the authenticated user.");
        }

        var homeId = Convert.ToInt32(me.Homes.Single().Id ?? 0);
        var zones = await _tadoService.GetZones(homeId);

        if (zones == null || zones.Length == 0)
        {
            return NotFound("No zones found for the home.");
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

        return Ok(new ActualTadoData
        {
            HomeId = homeId,
            InsideTemperatureCelsius = insiteTemperature,
            HumidityPercentage = humidityPercentage,
            TokenId = newToken.TokenId,
            ZoneName = $"{zoneName}",
        }); // Returns a 200 OK status with the JSON object
    }

    [Route("addSchedule")] // This makes the URL: api/tadotemperature/addSchedule
    [HttpPost]
    public async Task<ActionResult<CreatedTadoSchedule>> AddTrackingSchedule([FromBody] SetSchedule setSchedule)
    {
        var token = await _dbContext.TadoTokens.FirstOrDefaultAsync(t => t.TokenId == setSchedule.TokenId);
        if (token == null)
        {
            return NotFound("Token not found. Please authenticate first to obtain a valid token.");
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
            return Unauthorized("Failed to authenticate with Tado.");
        }

        var me = await _tadoService.GetMe();

        if (me == null || me.Homes == null || me.Homes.Length == 0)
        {
            return NotFound("No homes found for the authenticated user.");
        }

        var homeId = Convert.ToInt32(me.Homes.Single().Id ?? 0);
        var zones = await _tadoService.GetZones(homeId);

        if (zones == null || zones.Length == 0)
        {
            return NotFound("No zones found for the home.");
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

        if (setSchedule.IntervalInMinutes < 1)
        {
            setSchedule.IntervalInMinutes = 1;
        }
        if (setSchedule.IntervalInMinutes > 120)
        {
            setSchedule.IntervalInMinutes = 120;
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

    [Route("getCurrentSchedule")] // This makes the URL: api/tadotemperature/getCurrentSchedule
    [HttpGet]
    public async Task<ActionResult<TadoRetrievalScheduleModel>> GetCurrentSchedule()
    {
        var activeSchedule = await _dbContext.TadoRetrievalSchedules.OrderBy(s => s.ScheduleId).FirstOrDefaultAsync(s => s.IsActive);
        if (activeSchedule != null)
        {
            return Ok(activeSchedule.ConvertToModel());

        }
        var mostRecentSchedule = await _dbContext.TadoRetrievalSchedules.OrderByDescending(s => s.ScheduleId).FirstOrDefaultAsync();
        if (mostRecentSchedule != null)
        {
            return Ok(mostRecentSchedule.ConvertToModel());
        }

        return NotFound("No schedule found.");
    }

    [Route("editSchedule/{id}")]
    [HttpPut]
    public ActionResult<TadoRetrievalScheduleModel> EditSchedule(int id, [FromBody] TadoRetrievalScheduleModel updatedSchedule)
    {
        if (updatedSchedule == null)
        {
            return BadRequest("Schedule is required.");
        }

        var schedule = _dbContext.TadoRetrievalSchedules.SingleOrDefault(t => t.ScheduleId == id);
        if (schedule == null)
        {
            return NotFound();
        }

        if (updatedSchedule.Interval < 1)
        {
            updatedSchedule.Interval = 1;
        }
        if (updatedSchedule.Interval > 120)
        {
            updatedSchedule.Interval = 120;
        }

        schedule.IsActive = updatedSchedule.IsActive;
        schedule.Interval = updatedSchedule.Interval;

        _dbContext.SaveChanges();

        return Ok(schedule);
    }

}