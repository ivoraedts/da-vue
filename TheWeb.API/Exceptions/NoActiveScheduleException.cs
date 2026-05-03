namespace TheWeb.API.Exceptions;

public class NoActiveScheduleException : Exception
{
    public NoActiveScheduleException() : base("No active retrieval schedule found.")
    {
    }
}
