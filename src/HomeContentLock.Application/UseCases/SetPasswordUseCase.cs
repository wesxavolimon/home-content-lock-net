using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for setting or updating the blocker password.
/// </summary>
public class SetPasswordUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly IPasswordValidator _validator;
    private readonly ILogger _logger;

    public SetPasswordUseCase(IBlockerRepository repository, IPasswordValidator validator, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case.
    /// Note: The actual password hashing is delegated to the IPasswordValidator implementation.
    /// </summary>
    public async Task<Result> ExecuteAsync(string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.Warning("Set password attempted with empty password");
                return Result.Failure("Password cannot be empty");
            }

            if (newPassword.Length < 4)
            {
                _logger.Warning("Set password attempted with password too short");
                return Result.Failure("Password must be at least 4 characters");
            }

            // Hash the password and save it
            // Note: The implementation details are handled by IPasswordValidator
            // This use case doesn't need to know about SHA256 or file locations
            var hashPassword = HashPassword(newPassword);
            await _validator.SavePasswordHashAsync(hashPassword);

            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "PASSWORD_CHANGED",
                Status = "SUCCESS",
                Details = "Password updated successfully"
            };

            await _repository.SaveLogAsync(log);

            _logger.Information("Password updated successfully");
            return Result.Success("Password updated successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error setting password");
            return Result.Failure("Failed to set password", ex);
        }
    }

    /// <summary>
    /// Simple SHA256 hash implementation for password hashing.
    /// In production, should use proper key derivation functions like PBKDF2 or Argon2.
    /// </summary>
    private static string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }
}
