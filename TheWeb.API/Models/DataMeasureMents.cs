namespace TheWeb.API.Models
{
    public class DataMeasureMents
    {
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }
        public required DateTime RetrievedAt { get; set; }
    }
}