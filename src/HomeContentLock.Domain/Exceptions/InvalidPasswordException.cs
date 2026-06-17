namespace HomeContentLock.Domain.Exceptions;

public class InvalidPasswordException : BlockerException
{
    public InvalidPasswordException(string message = "Password validation failed")
        : base(message) { }
}
