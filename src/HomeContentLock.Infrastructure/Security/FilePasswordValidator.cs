using System.Security.Cryptography;
using System.Text;
using HomeContentLock.Domain.Interfaces;
using Serilog;

namespace HomeContentLock.Infrastructure.Security;

/// <summary>
/// SHA-256 based password validator using file-based storage.
/// Stores password hash in ~/.config/HomeContentLock/secrets.json
/// </summary>
public class FilePasswordValidator : IPasswordValidator
{
    private readonly SecretsFile _secretsFile;
    private readonly ILogger _logger;
    private const string Algorithm = "SHA256";

    public FilePasswordValidator(SecretsFile secretsFile, ILogger logger)
    {
        _secretsFile = secretsFile ?? throw new ArgumentNullException(nameof(secretsFile));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ValidateAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.Warning("Validation attempted with null or empty password");
                return false;
            }

            var secrets = await _secretsFile.ReadAsync();
            if (secrets == null)
            {
                _logger.Warning("No secrets file found; validation failed");
                return false;
            }

            var inputHash = HashPassword(password);
            var isValid = inputHash == secrets.PasswordHash;

            if (!isValid)
            {
                _logger.Warning("Password validation failed");
            }
            else
            {
                _logger.Information("Password validation succeeded");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating password");
            throw;
        }
    }

    public async Task SavePasswordHashAsync(string passwordHash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

            var secrets = new SecretsData
            {
                Version = "1.0",
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Algorithm = Algorithm
            };

            await _secretsFile.WriteAsync(secrets);
            _logger.Information("Password hash saved successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving password hash");
            throw;
        }
    }

    public async Task SetPasswordAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var hash = HashPassword(password);
            await SavePasswordHashAsync(hash);
            _logger.Information("Password set successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error setting password");
            throw;
        }
    }

    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }
}
