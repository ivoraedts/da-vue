using KoenZomers.Tado.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using TheWeb.API.Data;
using TheWebApi.Models;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]/get-new-url")] // This makes the URL: api/tadotemperature/get-new-url
public class TadoTemperatureController : ControllerBase
{
    private readonly Tado _tadoService;
    private readonly DaVueDbContext _dbContext;

    public TadoTemperatureController(Tado tado, DaVueDbContext dbContext)
    {
        _tadoService = tado;
        _dbContext = dbContext;
    }

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
}