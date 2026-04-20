using Microsoft.AspNetCore.Mvc;
using TheWebApi.Models;
using KoenZomers.Tado.Api.Controllers;


namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]/get-new-url")] // This makes the URL: api/tadotemperature/get-new-url
public class TadoTemperatureController : ControllerBase
{
    private readonly Tado _tadoService;

    public TadoTemperatureController(Tado tado)
    {
        _tadoService = tado;
    }

    [HttpGet]
    public async Task<ActionResult<TheWebApi.Models.TadoInitialization>> Get()
    {

        var deviceAuthorization = await _tadoService.GetDeviceCodeAuthentication(CancellationToken.None);
        if (deviceAuthorization==null)
        {
            return NotFound();
        }

        var fakeResult = new TadoInitialization
        {
            CommunicationId = 1235,
            VerificationUriComplete = $"{deviceAuthorization.VerificationUriComplete}",
            UserCode = $"{deviceAuthorization.UserCode}"
        };
        return Ok(fakeResult); // Returns a 200 OK status with the JSON object
    }
}