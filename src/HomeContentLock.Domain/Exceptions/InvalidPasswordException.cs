namespace HomeContentLock.Domain.Exceptions;

/// <summary>
/// Exception thrown when password validation fails.
/// </summary>
public class InvalidPasswordException : BlockerException
{
    public InvalidPasswordException(string message) : base(message)
    {
    }

    public InvalidPasswordException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
