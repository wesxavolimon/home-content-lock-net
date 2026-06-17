namespace HomeContentLock.Domain.Interfaces;

public interface IPasswordValidator
{
    Task<bool> ValidateAsync(string password);
    Task SavePasswordHashAsync(string passwordHash);
}
