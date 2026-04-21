namespace TheWebApi.Models
{
    public class ActualTadoData
    {
        public required int HomeId { get; set; }
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }
        public required int TokenId { get; set; }
        public required string ZoneName { get; set; }
    }
}