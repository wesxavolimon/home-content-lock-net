namespace HomeContentLock.Domain.Exceptions;

public class DatabaseException : BlockerException
{
    public DatabaseException(string message = "Database operation failed")
        : base(message) { }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException) { }
}
