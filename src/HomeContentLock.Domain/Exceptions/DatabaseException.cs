namespace HomeContentLock.Domain.Exceptions;

/// <summary>
/// Exception thrown when database operation fails.
/// </summary>
public class DatabaseException : BlockerException
{
    public DatabaseException(string message) : base(message)
    {
    }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
