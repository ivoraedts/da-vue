using Microsoft.EntityFrameworkCore;
using TheWeb.API.Data;

namespace TheWeb.API.Services;

public interface IHourlyDataAggregationService
{
    Task AggregateDataAsync(CancellationToken cancellationToken);
}

public class HourlyDataAggregationService(ILogger<DataRetrievalService> logger, DaVueDbContext dbContext)
    : IHourlyDataAggregationService
{
    public async Task AggregateDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lastRetrievalDateTime = await dbContext.TadoRetrievedData.OrderByDescending(d => d.RetrievalId)
                .Select(d => d.RetrievedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if  (lastRetrievalDateTime == default) return;
            
            var lastHourAggregated = await dbContext.HourlyAggregations.OrderByDescending(a=>a.AggregationId)
                .Select(a=>a.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (lastHourAggregated == default)
            {
                lastHourAggregated = await ArrangeStartingPoint(lastHourAggregated, cancellationToken);
            }
            var nextHourToAggregate = lastHourAggregated = lastHourAggregated.AddHours(1);

            while (nextHourToAggregate.AddHours(1)<lastRetrievalDateTime)
            {
                await AggregateDataForHour(nextHourToAggregate, cancellationToken);
                nextHourToAggregate = nextHourToAggregate.AddHours(1);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while aggregating: {ex}");
            Console.WriteLine(ex);
            throw;
        }
    }

    private async Task<DateTime> ArrangeStartingPoint(DateTime lastHourAggregated, CancellationToken cancellationToken)
    {
        //no aggregation was ever completed, so we need to start at the front
        var firstRetrievalDateTime= await dbContext.TadoRetrievedData.OrderBy(d => d.RetrievalId)
            .Select(d => d.RetrievedAt)
            .FirstAsync(cancellationToken);
        lastHourAggregated = new DateTime(
            firstRetrievalDateTime.Year, 
            firstRetrievalDateTime.Month, 
            firstRetrievalDateTime.Day, 
            firstRetrievalDateTime.Hour, 
            0, 0, 0, firstRetrievalDateTime.Kind // Preserves UTC vs Local
        );
        return lastHourAggregated;
    }
    
    private async Task AggregateDataForHour(DateTime lastHourAggregated, CancellationToken cancellationToken)
    {
        var start = lastHourAggregated;
        var stop = lastHourAggregated.AddHours(1);
        
        var entriesToAggregate = await dbContext.TadoRetrievedData.Where(
            d => d.RetrievedAt >= start && d.RetrievedAt < stop)
            .ToListAsync(cancellationToken);

        if (entriesToAggregate.Count == 0)
        {
            logger.LogWarning($"Faking data for {lastHourAggregated}...");
            var previousEntry = await dbContext.TadoRetrievedData
                .OrderByDescending(d => d.RetrievalId)
                .Where(d => d.RetrievedAt < start)
                .FirstOrDefaultAsync(cancellationToken);

            if (previousEntry == null)
            {
                var error = $"Could not find any data before {start}...";
                logger.LogError(error);
                throw new InvalidOperationException(error);
            }
                
            var nextEntry = await dbContext.TadoRetrievedData
                .OrderBy(d => d.RetrievalId)
                .Where(d => d.RetrievedAt > stop)
                .FirstOrDefaultAsync(cancellationToken);
             
            if (nextEntry == null)
            {
                var error = $"Could not find any data after {stop}... (so not even after {start})";
                logger.LogError(error);
                throw new InvalidOperationException(error);
            }
            
            entriesToAggregate.Add(previousEntry);
            entriesToAggregate.Add(nextEntry);
        }
        
        var averageTemperature = entriesToAggregate.Average(d => d.InsideTemperatureCelsius);
        var averageHumidity = entriesToAggregate.Average(d => d.HumidityPercentage);
        dbContext.HourlyAggregations.Add(new RetrievalAggregation
        {
            TimeStamp =  lastHourAggregated,
            InsideTemperatureCelsius =  Math.Round(averageTemperature,2),
            HumidityPercentage =  Math.Round(averageHumidity,1)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}