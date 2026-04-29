using System.ComponentModel.DataAnnotations;
namespace TheWeb.API.Data;

public class TadoRetrievedData
{
    [Key]
    public int RetrievalId { get; set; }
    public required double InsideTemperatureCelsius { get; set; }
    public required double HumidityPercentage { get; set; }    
    public required DateTime RetrievedAt { get; set; }
}