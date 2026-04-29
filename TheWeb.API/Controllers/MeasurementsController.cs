using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;
using TheWeb.API.Models;

namespace TheWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class MeasurementsController : ControllerBase
{
    private readonly DaVueDbContext _dbContext;

    public MeasurementsController(DaVueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Route("latest")] // This makes the URL: api/measurements/latest
    [HttpGet]
    public async Task<ActionResult<DataMeasureMents>> GetLatestMeasurements()
    {
        var latestMeasurement = await _dbContext.TadoRetrievedData.OrderByDescending(data=>data.RetrievalId).FirstOrDefaultAsync();
        if (latestMeasurement == null)
        {
            return NotFound("No measurements found.");
        }

        return Ok(new DataMeasureMents
        {
            InsideTemperatureCelsius = latestMeasurement.InsideTemperatureCelsius,
            HumidityPercentage = latestMeasurement.HumidityPercentage,
            RetrievedAt = latestMeasurement.RetrievedAt
        });
    }

    [Route("last10")] // This makes the URL: api/measurements/last10
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetLast10Measurements()
    {
        var latestMeasurements = await _dbContext.TadoRetrievedData.OrderByDescending(data => data.RetrievalId).Take(10).ToListAsync();
        if (latestMeasurements == null || latestMeasurements.Count == 0)
        {
            return NotFound("No measurements found.");
        }

        var result = latestMeasurements.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.RetrievedAt
        }).ToList();

        return Ok(result);
    }
    
    [Route("hourly")] // This makes the URL: api/measurements/hourly
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetLastHourlyDataAggregations()
    {
        var aggregations = await _dbContext.HourlyAggregations.OrderByDescending(data => data.AggregationId).Take(24).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No measurements found.");
        }

        var result = aggregations.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp
        }).Reverse().ToList();

        return Ok(result);
    }
}
