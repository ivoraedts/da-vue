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
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp
        }).Reverse().ToList();

        return Ok(result);
    }
    
    [Route("hourly/boundaries")] // This makes the URL: api/measurements/hourly/boundaries
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetLastHourlyDataAggregationBoundaries()
    {
        var firstAggregation = await _dbContext.HourlyAggregations.OrderBy(data => data.AggregationId).FirstOrDefaultAsync();
        if (firstAggregation == null)
        {
            return NotFound("No aggregation found.");
        }
        var lastAggregation = await _dbContext.HourlyAggregations.OrderByDescending(data => data.AggregationId).FirstAsync();


        var result = new Boundaries
        {
            OldestItem = firstAggregation.TimeStamp,
            NewestItem = lastAggregation.TimeStamp,
        };

        return Ok(result);
    }
    
    [Route("hourly/{date}")] // This makes the URL: api/measurements/hourly/{date}
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetLastHourlyDataAggregations(DateTime date)
    {
        date = date.AddDays(1);
        var aggregations = await _dbContext.HourlyAggregations
            .OrderByDescending(data => data.AggregationId)
            .Where(data => data.TimeStamp < date)
            .Take(24).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp
        }).Reverse().ToList();

        return Ok(result);
    }

    [Route("daily")] // This makes the URL: api/measurements/daily
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetLastDailyDataAggregations(int take = 7)
    {
        if (take <= 0)
        {
            return BadRequest("Parameter 'take' must be greater than zero.");
        }

        var aggregations = await _dbContext.DailyAggregations.OrderByDescending(data => data.AggregationId).Take(take).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp
        }).Reverse().ToList();

        return Ok(result);
    }

    [Route("daily/boundaries")] // This makes the URL: api/measurements/daily/boundaries
    [HttpGet]
    public async Task<ActionResult<Boundaries>> GetLastDailyDataAggregationBoundaries()
    {
        var firstAggregation = await _dbContext.DailyAggregations.OrderBy(data => data.AggregationId).FirstOrDefaultAsync();
        if (firstAggregation == null)
        {
            return NotFound("No aggregation found.");
        }
        var lastAggregation = await _dbContext.DailyAggregations.OrderByDescending(data => data.AggregationId).FirstAsync();

        var result = new Boundaries
        {
            OldestItem = firstAggregation.TimeStamp,
            NewestItem = lastAggregation.TimeStamp,
        };

        return Ok(result);
    }

    [Route("daily/{date}")] // This makes the URL: api/measurements/daily/{date}
    [HttpGet]
    public async Task<ActionResult<List<DataMeasureMents>>> GetDailyDataAggregations(DateTime date, int take = 7)
    {
        if (take <= 0)
        {
            return BadRequest("Parameter 'take' must be greater than zero.");
        }

        date = date.AddDays(1);
        var aggregations = await _dbContext.DailyAggregations
            .OrderByDescending(data => data.AggregationId)
            .Where(data => data.TimeStamp < date)
            .Take(take).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DataMeasureMents
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp
        }).Reverse().ToList();

        return Ok(result);
    }

    [Route("daypart")] // This makes the URL: api/measurements/daypart
    [HttpGet]
    public async Task<ActionResult<List<DatePartMeasurements>>> GetLastDayPartDataAggregations(int take = 28)
    {
        if (take <= 0)
        {
            return BadRequest("Parameter 'take' must be greater than zero.");
        }

        var aggregations = await _dbContext.DayPartAggregations.OrderByDescending(data => data.AggregationId).Take(take).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DatePartMeasurements
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp,
            DayPart = m.DayPart
        }).Reverse().ToList();

        return Ok(result);
    }

    [Route("daypart/boundaries")] // This makes the URL: api/measurements/daypart/boundaries
    [HttpGet]
    public async Task<ActionResult<Boundaries>> GetLastDayPartDataAggregationBoundaries()
    {
        var firstAggregation = await _dbContext.DayPartAggregations.OrderBy(data => data.AggregationId).FirstOrDefaultAsync();
        if (firstAggregation == null)
        {
            return NotFound("No aggregation found.");
        }
        var lastAggregation = await _dbContext.DayPartAggregations.OrderByDescending(data => data.AggregationId).FirstAsync();

        var result = new Boundaries
        {
            OldestItem = firstAggregation.TimeStamp,
            NewestItem = lastAggregation.TimeStamp,
        };

        return Ok(result);
    }

    [Route("daypart/{date}")] // This makes the URL: api/measurements/daypart/{date}
    [HttpGet]
    public async Task<ActionResult<List<DatePartMeasurements>>> GetDayPartDataAggregations(DateTime date, int take = 28)
    {
        if (take <= 0)
        {
            return BadRequest("Parameter 'take' must be greater than zero.");
        }

        date = date.AddDays(1);
        var aggregations = await _dbContext.DayPartAggregations
            .OrderByDescending(data => data.AggregationId)
            .Where(data => data.TimeStamp < date)
            .Take(take).ToListAsync();
        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found.");
        }

        var result = aggregations.Select(m => new DatePartMeasurements
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp,
            DayPart = m.DayPart
        }).Reverse().ToList();

        return Ok(result);
    }

    [Route("daypart/month/{year}/{month}")] // This makes the URL: api/measurements/daypart/month/{year}/{month}
    [HttpGet]
    public async Task<ActionResult<List<DatePartMeasurements>>> GetDayPartDataAggregationsByMonth(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            return BadRequest("Month must be between 1 and 12.");
        }

        var aggregations = await _dbContext.DayPartAggregations
            .OrderByDescending(data => data.AggregationId)
            .Where(data => data.TimeStamp.Year == year && data.TimeStamp.Month == month)
            .ToListAsync();

        if (aggregations == null || aggregations.Count == 0)
        {
            return NotFound("No aggregation found for the requested month.");
        }

        var result = aggregations.Select(m => new DatePartMeasurements
        {
            InsideTemperatureCelsius = m.InsideTemperatureCelsius,
            HumidityPercentage = m.HumidityPercentage,
            RetrievedAt = m.TimeStamp,
            DayPart = m.DayPart
        }).Reverse().ToList();

        return Ok(result);
    }
}
