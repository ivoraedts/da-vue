namespace TheWeb.API.Exceptions;

public class TadoApiNotRespondingException : Exception
{
    public int NumberOfConsecutiveFailures { get; set; }

    public TadoApiNotRespondingException(int numberOfConsecutiveFailures, string message) : base(message)
    {
        NumberOfConsecutiveFailures = numberOfConsecutiveFailures;
    }
}
