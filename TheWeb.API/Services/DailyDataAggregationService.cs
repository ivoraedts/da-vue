using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;

namespace TheWeb.API.Services;

public interface IDailyDataAggregationService
{
    Task AggregateDataAsync(CancellationToken cancellationToken);
}

public class DailyDataAggregationService(ILogger<DailyDataAggregationService> logger, DaVueDbContext dbContext)
    : IDailyDataAggregationService
{
    public async Task AggregateDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lastHourlyAggregationTime = await dbContext.HourlyAggregations.OrderByDescending(h => h.AggregationId)
                .Select(h => h.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastHourlyAggregationTime == default) return;

            var lastDayAggregated = await dbContext.DailyAggregations.OrderByDescending(a => a.AggregationId)
                .Select(a => a.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastDayAggregated == default)
            {
                lastDayAggregated = await ArrangeStartingPoint(cancellationToken);
            }

            var nextDayToAggregate = lastDayAggregated = lastDayAggregated.AddDays(1);

            while (nextDayToAggregate.AddDays(1) <= lastHourlyAggregationTime.AddHours(1))
            {
                await AggregateDataForDay(nextDayToAggregate, cancellationToken);
                nextDayToAggregate = nextDayToAggregate.AddDays(1);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while aggregating daily data: {ex}");
            Console.WriteLine(ex);
            throw;
        }
    }

    private async Task<DateTime> ArrangeStartingPoint(CancellationToken cancellationToken)
    {
        var firstHourlyAggregationTime = await dbContext.HourlyAggregations.OrderBy(h => h.AggregationId)
            .Select(h => h.TimeStamp)
            .FirstAsync(cancellationToken);

        return new DateTime(
            firstHourlyAggregationTime.Year,
            firstHourlyAggregationTime.Month,
            firstHourlyAggregationTime.Day,
            0, 0, 0, firstHourlyAggregationTime.Kind
        );
    }

    private async Task AggregateDataForDay(DateTime lastDayAggregated, CancellationToken cancellationToken)
    {
        var start = lastDayAggregated;
        var stop = lastDayAggregated.AddDays(1);

        var entriesToAggregate = await dbContext.HourlyAggregations.Where(
            h => h.TimeStamp >= start && h.TimeStamp < stop)
            .ToListAsync(cancellationToken);

        if (entriesToAggregate.Count == 0)
        {
            logger.LogWarning($"Faking daily data for {lastDayAggregated:O}...");
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

        dbContext.DailyAggregations.Add(new DailyRetrievalAggregation
        {
            TimeStamp = lastDayAggregated,
            InsideTemperatureCelsius = Math.Round(averageTemperature, 2),
            HumidityPercentage = Math.Round(averageHumidity, 1)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
