using System.ComponentModel.DataAnnotations;
namespace TheWeb.API.Data;

public class TadoRetrievalSchedule
{
    [Key]
    public int ScheduleId { get; set; }
    public required int TokenId { get; set; }
    public required int Interval { get; set; }
    public required DateTime NextRetrievalTime { get; set; }
    public required DateTime LastRetrievalTime { get; set; }
    public bool IsActive { get; set; }
    public required int HomeId { get; set; }
    public required string ZoneName { get; set; }
    public required string LastError { get; set; }
    public required int ConsecutiveFailures { get; set; }
    public string? Password { get; set; }
}