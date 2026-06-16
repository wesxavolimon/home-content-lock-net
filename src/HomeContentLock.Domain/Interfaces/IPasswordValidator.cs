namespace HomeContentLock.Domain.Interfaces;

/// <summary>
/// Interface for password validation and hashing.
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Validate a password against the stored hash.
    /// </summary>
    Task<bool> ValidateAsync(string password);

    /// <summary>
    /// Save a new password hash to persistent storage.
    /// </summary>
    Task SavePasswordHashAsync(string passwordHash);
}
