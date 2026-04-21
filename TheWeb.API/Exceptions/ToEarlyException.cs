namespace TheWeb.API.Exceptions;

public class ToEarlyException : Exception
{
    public DateTime NextRetrievalTime { get; set; }

    public ToEarlyException(DateTime nextRetrievalTime, string message) : base(message)
    {
        NextRetrievalTime = nextRetrievalTime;
    }
}
