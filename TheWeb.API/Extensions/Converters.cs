using KoenZomers.Tado.Api.Models.Authentication;
using TheWeb.API.Data;
using TheWeb.API.Models;

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

        public static TadoRetrievalScheduleModel ConvertToModel(this TadoRetrievalSchedule schedule)
        {
            return new TadoRetrievalScheduleModel
            {
                ScheduleId = schedule.ScheduleId,
                TokenId = schedule.TokenId,
                Interval = schedule.Interval,
                NextRetrievalTime = schedule.NextRetrievalTime,
                LastRetrievalTime = schedule.LastRetrievalTime,
                IsActive = schedule.IsActive,
                HomeId = schedule.HomeId,
                ZoneName = schedule.ZoneName,
                LastError = schedule.LastError,
                ConsecutiveFailures = schedule.ConsecutiveFailures
            };
        }

        public static double CelsiusToFahrenheit(double celsius)
        {
            return (celsius * 9 / 5) + 32;
        }
    }
}