namespace HomeContentLock.Domain.Exceptions;

public class BlockerException : Exception
{
    public BlockerException(string message) : base(message) { }

    public BlockerException(string message, Exception innerException)
        : base(message, innerException) { }
}
