namespace HomeContentLock.Domain.Exceptions;

/// <summary>
/// Base exception for blocker-related operations.
/// </summary>
public class BlockerException : Exception
{
    public BlockerException(string message) : base(message)
    {
    }

    public BlockerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
