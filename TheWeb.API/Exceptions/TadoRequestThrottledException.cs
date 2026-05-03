namespace TheWeb.API.Exceptions;

public class TadoRequestThrottledException : Exception
{
    public int NumberOfConsecutiveFailures { get; set; }

    public TadoRequestThrottledException(int numberOfConsecutiveFailures, string message) : base(message)
    {
        NumberOfConsecutiveFailures = numberOfConsecutiveFailures;
    }
}
