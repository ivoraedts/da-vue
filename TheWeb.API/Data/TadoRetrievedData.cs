using System.ComponentModel.DataAnnotations;
namespace TheWeb.API.Data;

public class TadoRetrievedData
{
    [Key]
    public int RetrievalId { get; set; }
    public required int ScheduleId { get; set; }
    public required int HomeId { get; set; } // currently useless, but could be used for future multi-home support
    public required string ZoneName { get; set; } // currently useless, but could be used for future multi-zone support
    public required double InsideTemperatureCelsius { get; set; }
    public required double HumidityPercentage { get; set; }    
    public required DateTime RetrievedAt { get; set; }
}