namespace TheWeb.API.Models
{
    public class LatestMeasurement
    {
        public required int RetrievalId { get; set; }
        public required int HomeId { get; set; }
        public required string ZoneName { get; set; }
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }
        public required DateTime RetrievedAt { get; set; }
    }
}