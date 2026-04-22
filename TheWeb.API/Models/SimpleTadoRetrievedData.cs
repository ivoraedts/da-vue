namespace TheWeb.API.Models
{
    public class SimpleTadoRetrievedData
    {
        public required int HomeId { get; set; }
        public required string ZoneName { get; set; }
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; } 
    }
}