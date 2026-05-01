using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheWeb.API.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DayPart
{
    Night,
    Morning,
    Afternoon,
    Evening
}

public class DayPartRetrievalAggregation
{
    [Key]
    public int AggregationId { get; set; }
    public required double InsideTemperatureCelsius { get; set; }
    public required double HumidityPercentage { get; set; }
    public required DateTime TimeStamp { get; set; }
    public required DayPart DayPart { get; set; }
}