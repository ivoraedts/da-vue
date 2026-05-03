using System.ComponentModel.DataAnnotations;
namespace TheWeb.API.Data;

public class TadoToken
{
    [Key]
    public int TokenId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required int ExpiresIn { get; set; }
    public required string Scope { get; set; }
    public required string TokenType { get; set; }
    public required string UserId { get; set; }
}
