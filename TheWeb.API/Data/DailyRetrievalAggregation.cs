using System.ComponentModel.DataAnnotations;

namespace TheWeb.API.Data;

public class DailyRetrievalAggregation
{
    [Key]
    public int AggregationId { get; set; }
    public required double InsideTemperatureCelsius { get; set; }
    public required double HumidityPercentage { get; set; }
    public required DateTime TimeStamp { get; set; }
}
