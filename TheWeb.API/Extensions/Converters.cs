using KoenZomers.Tado.Api.Models.Authentication;
using TheWeb.API.Data;

namespace TheWeb.API.Extensions
{
    public static class Converters
    {
        public static Token ConvertToTadoToken(this TadoToken token)
        {
            return new Token
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn,
                Scope = token.Scope,
                TokenType = token.TokenType,
                UserId = token.UserId
            };
        }

        public static double CelsiusToFahrenheit(double celsius)
        {
            return (celsius * 9 / 5) + 32;
        }
    }
}