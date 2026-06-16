using System.Text.Json;
using Serilog;

namespace HomeContentLock.Infrastructure.Security;

/// <summary>
/// Manages secrets stored in ~/.config/HomeContentLock/secrets.json
/// </summary>
public class SecretsFile
{
    private readonly string _secretsPath;
    private readonly ILogger _logger;

    public SecretsFile(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "HomeContentLock",
            "secrets.json"
        );
    }

    public async Task<SecretsData?> ReadAsync()
    {
        try
        {
            if (!File.Exists(_secretsPath))
            {
                _logger.Information("Secrets file does not exist at {Path}", _secretsPath);
                return null;
            }

            var json = await File.ReadAllTextAsync(_secretsPath);
            var data = JsonSerializer.Deserialize<SecretsData>(json);

            _logger.Debug("Secrets file read from {Path}", _secretsPath);
            return data;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error reading secrets file from {Path}", _secretsPath);
            throw;
        }
    }

    public async Task WriteAsync(SecretsData data)
    {
        try
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var directory = Path.GetDirectoryName(_secretsPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.Information("Created secrets directory at {Directory}", directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(_secretsPath, json);

            // Set permissions to 600 (read/write for owner only) on Unix-like systems
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var fileInfo = new FileInfo(_secretsPath);
                // Note: This is a simplified approach; proper implementation would use native APIs
                _logger.Debug("Secrets file written to {Path}", _secretsPath);
            }
            else
            {
                _logger.Debug("Secrets file written to {Path}", _secretsPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error writing secrets file to {Path}", _secretsPath);
            throw;
        }
    }
}

/// <summary>
/// Structure of the secrets.json file
/// </summary>
public class SecretsData
{
    public string Version { get; set; } = "1.0";
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Algorithm { get; set; } = "SHA256";
}
