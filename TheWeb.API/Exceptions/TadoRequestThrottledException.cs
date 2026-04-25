namespace TheWeb.API.Exceptions;

public class TadoRequestThrottledException : Exception
{
    public int NumberOfConsecutiveFailures { get; set; }

    public TadoRequestThrottledException(int NumberOfConsecutiveFailures, string message) : base(message)
    {
        NumberOfConsecutiveFailures = NumberOfConsecutiveFailures;
    }
}
