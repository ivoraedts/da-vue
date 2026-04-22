namespace TheWeb.API.Models
{
    public class TadoInitialization
    {
        public required int CommunicationId { get; set; }
        public required string VerificationUriComplete { get; set; }
        public required string UserCode { get; set; }
    }
}