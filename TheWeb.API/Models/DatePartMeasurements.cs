using TheWeb.API.Data;

namespace TheWeb.API.Models;

public class DatePartMeasurements
{
    public required double InsideTemperatureCelsius { get; set; }
    public required double HumidityPercentage { get; set; }
    public required DateTime RetrievedAt { get; set; }
    public required DayPart DayPart { get; set; }
}
