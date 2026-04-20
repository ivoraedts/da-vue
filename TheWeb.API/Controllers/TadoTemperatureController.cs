using Microsoft.AspNetCore.Mvc;
using TheWebApi.Models;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]/get-new-url")] // This makes the URL: api/tadotemperature/get-new-url
public class TadoTemperatureController : ControllerBase
{
    [HttpGet]
    public ActionResult<TheWebApi.Models.TadoInitialization> Get()
    {
        var fakeResult = new TadoInitialization
        {
            CommunicationId = 12345,
            VerificationUriComplete = "https://example.com/verification",
            UserCode = "ABCDE"
        };
        return Ok(fakeResult); // Returns a 200 OK status with the JSON object
    }
}