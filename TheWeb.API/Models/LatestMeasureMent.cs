namespace TheWeb.API.Models
{
    public class LatestMeasurement
    {
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }
        public required DateTime RetrievedAt { get; set; }
    }
}