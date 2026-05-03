using System.ComponentModel.DataAnnotations;

namespace TheWeb.API.Data;

public class TadoDeviceAuthentication
{
    [Key]
    public int CommunicationId { get; set; }
    public required DateTime Creation { get; set;}
    public required string DeviceCode { get; set; }
    public short ExpiresIn { get; set; }
    public short Interval { get; set; }
    public required string UserCode { get; set; }
    public required string VerificationUri { get; set; }
    public required string VerificationUriComplete { get; set; }
}
