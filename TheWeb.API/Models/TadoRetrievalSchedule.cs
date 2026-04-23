namespace TheWeb.API.Models
{
    public class TadoRetrievalScheduleModel
    {
    public int ScheduleId { get; set; }
    public int TokenId { get; set; }
    public int Interval { get; set; }
    public required DateTime NextRetrievalTime { get; set; }
    public required DateTime LastRetrievalTime { get; set; }
    public bool IsActive { get; set; }
    public int HomeId { get; set; }
    public required string ZoneName { get; set; }
    public required string LastError { get; set; }
    public int ConsecutiveFailures { get; set; }
    }
}