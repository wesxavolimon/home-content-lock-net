using System.Security.Cryptography;
using System.Text;
using HomeContentLock.Domain.Exceptions;
using HomeContentLock.Infrastructure.Security;
using Serilog;

namespace HomeContentLock.Infrastructure.Tests.Security;

public class FilePasswordValidatorTests : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _testSecretsDir;
    private readonly SecretsFile _secretsFile;
    private readonly FilePasswordValidator _validator;

    public FilePasswordValidatorTests()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create a temporary directory for test secrets
        _testSecretsDir = Path.Combine(Path.GetTempPath(), $"secrets_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testSecretsDir);

        _secretsFile = new SecretsFile(_logger);
        _validator = new FilePasswordValidator(_secretsFile, _logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSecretsDir))
        {
            Directory.Delete(_testSecretsDir, recursive: true);
        }
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }

    [Fact]
    public async Task ValidateAsync_WithNullPassword_ShouldReturnFalse()
    {
        // Act
        var result = await _validator.ValidateAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyPassword_ShouldReturnFalse()
    {
        // Act
        var result = await _validator.ValidateAsync(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateAsync_WithNoSecretsFile_ShouldReturnFalse()
    {
        // Act
        var result = await _validator.ValidateAsync("somepassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetPasswordAsync_WithValidPassword_ShouldSaveHash()
    {
        // Act
        await _validator.SetPasswordAsync("testpassword");

        // Assert
        var secrets = await _secretsFile.ReadAsync();
        Assert.NotNull(secrets);
        Assert.NotNull(secrets.PasswordHash);
        Assert.Equal("SHA256", secrets.Algorithm);
    }

    [Fact]
    public async Task SetPasswordAsync_WithNullPassword_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _validator.SetPasswordAsync(null));
    }

    [Fact]
    public async Task SetPasswordAsync_WithEmptyPassword_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _validator.SetPasswordAsync(string.Empty));
    }

    [Fact]
    public async Task ValidateAsync_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "correctpassword";
        await _validator.SetPasswordAsync(password);

        // Act
        var result = await _validator.ValidateAsync(password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string correctPassword = "correctpassword";
        const string wrongPassword = "wrongpassword";
        await _validator.SetPasswordAsync(correctPassword);

        // Act
        var result = await _validator.ValidateAsync(wrongPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetPasswordAsync_WithSamePassword_ShouldProduceSameHash()
    {
        // Arrange
        const string password = "testpassword";

        // Act
        await _validator.SetPasswordAsync(password);
        var firstHash = (await _secretsFile.ReadAsync()).PasswordHash;

        await _validator.SetPasswordAsync(password);
        var secondHash = (await _secretsFile.ReadAsync()).PasswordHash;

        // Assert (SHA256 of the same input should always produce the same output)
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public async Task ValidateAsync_WithDifferentCasePassword_ShouldFail()
    {
        // Arrange
        const string password = "TestPassword";
        const string differentCase = "testpassword";
        await _validator.SetPasswordAsync(password);

        // Act
        var result = await _validator.ValidateAsync(differentCase);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SavePasswordHashAsync_WithValidHash_ShouldCreateSecretsFile()
    {
        // Arrange
        var hash = HashPassword("testpassword");

        // Act
        await _validator.SavePasswordHashAsync(hash);

        // Assert
        var secrets = await _secretsFile.ReadAsync();
        Assert.NotNull(secrets);
        Assert.Equal(hash, secrets.PasswordHash);
    }

    [Fact]
    public async Task SavePasswordHashAsync_WithNullHash_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _validator.SavePasswordHashAsync(null));
    }

    [Fact]
    public async Task SavePasswordHashAsync_WithEmptyHash_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _validator.SavePasswordHashAsync(string.Empty));
    }
}
