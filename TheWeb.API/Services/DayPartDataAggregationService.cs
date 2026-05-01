using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;

namespace TheWeb.API.Services;

public interface IDayPartDataAggregationService
{
    Task AggregateDataAsync(CancellationToken cancellationToken);
}

public class DayPartDataAggregationService(ILogger<DayPartDataAggregationService> logger, DaVueDbContext dbContext, IConfiguration configuration)
    : IDayPartDataAggregationService
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Timezone"] ?? "Europe/Amsterdam");

    public async Task AggregateDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lastHourlyAggregationTime = await dbContext.HourlyAggregations.OrderByDescending(h => h.AggregationId)
                .Select(h => h.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastHourlyAggregationTime == default) return;

            var lastDayPartAggregated = await dbContext.DayPartAggregations.OrderByDescending(a => a.AggregationId)
                .Select(a => a.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastDayPartAggregated == default)
            {
                lastDayPartAggregated = await ArrangeStartingPoint(cancellationToken);
            }

            var nextDayPartToAggregate = lastDayPartAggregated = lastDayPartAggregated.AddHours(6);

            while (nextDayPartToAggregate.AddHours(6) <= lastHourlyAggregationTime.AddHours(1))
            {
                await AggregateDataForDayPart(nextDayPartToAggregate, cancellationToken);
                nextDayPartToAggregate = nextDayPartToAggregate.AddHours(6);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while aggregating day part data: {ex}");
            Console.WriteLine(ex);
            throw;
        }
    }

    private async Task<DateTime> ArrangeStartingPoint(CancellationToken cancellationToken)
    {
        var firstHourlyAggregationTime = await dbContext.HourlyAggregations.OrderBy(h => h.AggregationId)
            .Select(h => h.TimeStamp)
            .FirstAsync(cancellationToken);

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(firstHourlyAggregationTime, _timeZone);
        var segmentStartHour = (localTime.Hour / 6) * 6;
        var startOfDayPart = new DateTime(localTime.Year, localTime.Month, localTime.Day, segmentStartHour, 0, 0, localTime.Kind);
        return TimeZoneInfo.ConvertTimeToUtc(startOfDayPart, _timeZone);
    }

    private async Task AggregateDataForDayPart(DateTime lastDayPartAggregated, CancellationToken cancellationToken)
    {
        var start = lastDayPartAggregated;
        var stop = lastDayPartAggregated.AddHours(6);

        var entriesToAggregate = await dbContext.HourlyAggregations.Where(
            h => h.TimeStamp >= start && h.TimeStamp < stop)
            .ToListAsync(cancellationToken);

        if (entriesToAggregate.Count == 0)
        {
            logger.LogWarning($"Faking day part data for {lastDayPartAggregated:O}...");
            var previousEntry = await dbContext.HourlyAggregations
                .OrderByDescending(h => h.AggregationId)
                .Where(h => h.TimeStamp < start)
                .FirstOrDefaultAsync(cancellationToken);

            if (previousEntry == null)
            {
                var error = $"Could not find any hourly aggregation before {start:O}...";
                logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            var nextEntry = await dbContext.HourlyAggregations
                .OrderBy(h => h.AggregationId)
                .Where(h => h.TimeStamp > stop)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextEntry == null)
            {
                var error = $"Could not find any hourly aggregation after {stop:O}... (so not even after {start:O})";
                logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            entriesToAggregate.Add(previousEntry);
            entriesToAggregate.Add(nextEntry);
        }

        var averageTemperature = entriesToAggregate.Average(h => h.InsideTemperatureCelsius);
        var averageHumidity = entriesToAggregate.Average(h => h.HumidityPercentage);

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(lastDayPartAggregated, _timeZone);
        var dayPart = GetDayPart(localTime.Hour);

        dbContext.DayPartAggregations.Add(new DayPartRetrievalAggregation
        {
            TimeStamp = lastDayPartAggregated,
            InsideTemperatureCelsius = Math.Round(averageTemperature, 2),
            HumidityPercentage = Math.Round(averageHumidity, 1),
            DayPart = dayPart
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private DayPart GetDayPart(int hour)
    {
        if (hour >= 0 && hour < 6) return DayPart.Night;
        if (hour >= 6 && hour < 12) return DayPart.Morning;
        if (hour >= 12 && hour < 18) return DayPart.Afternoon;
        return DayPart.Evening;
    }
}