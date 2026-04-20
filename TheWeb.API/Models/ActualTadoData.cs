namespace TheWebApi.Models
{
    public class ActualTadoData
    {
        public required int HomeId { get; set; }
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}