namespace TheWeb.API.Models
{
    public class CreatedTadoSchedule
    {
        public required int ScheduleId { get; set; }
        public required int HomeId { get; set; }
        public required double InsideTemperatureCelsius { get; set; }
        public required double HumidityPercentage { get; set; }        
        public required string ZoneName { get; set; }
        public required int TokenId { get; set; }
        public required int Interval { get; set; }
        public required DateTime NextRetrievalTime { get; set; }
        public required DateTime LastRetrievalTime { get; set; }
    }
}