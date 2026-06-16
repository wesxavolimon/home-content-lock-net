using HomeContentLock.Domain.Entities;
using HomeContentLock.Infrastructure.Persistence;

namespace HomeContentLock.Infrastructure.Tests.Persistence;

public class SqliteBlockerRepositoryTests : IAsyncLifetime
{
    private TestDatabaseFixture _fixture = null!;
    private SqliteBlockerRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _fixture = new TestDatabaseFixture();
        await _fixture.InitializeAsync();
        _repository = new SqliteBlockerRepository(_fixture.Context, _fixture.Logger);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    #region Log Entry Tests

    [Fact]
    public async Task SaveLogAsync_WithValidLog_ShouldSaveSuccessfully()
    {
        // Arrange
        var log = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "TEST_ACTION",
            Status = "SUCCESS",
            Details = "Test details"
        };

        // Act
        await _repository.SaveLogAsync(log);

        // Assert
        var saved = await _repository.GetLogsAsync();
        Assert.NotEmpty(saved);
        Assert.Equal("TEST_ACTION", saved[0].Action);
    }

    [Fact]
    public async Task SaveLogAsync_WithNullLog_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveLogAsync(null!));
    }

    [Fact]
    public async Task GetLogsAsync_WithNoLogs_ShouldReturnEmptyList()
    {
        // Act
        var logs = await _repository.GetLogsAsync();

        // Assert
        Assert.Empty(logs);
    }

    [Fact]
    public async Task GetLogsAsync_WithMultipleLogs_ShouldReturnInDescendingOrder()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            var log = new LogEntry
            {
                Timestamp = baseTime.AddSeconds(i),
                Action = $"ACTION_{i}",
                Status = "SUCCESS"
            };
            await _repository.SaveLogAsync(log);
        }

        // Act
        var logs = await _repository.GetLogsAsync();

        // Assert
        Assert.Equal(5, logs.Count);
        // Should be in descending order (most recent first)
        for (int i = 0; i < logs.Count - 1; i++)
        {
            Assert.True(logs[i].Timestamp >= logs[i + 1].Timestamp);
        }
    }

    [Fact]
    public async Task GetLogsAsync_WithLimitGreaterThanCount_ShouldReturnAllLogs()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = $"ACTION_{i}",
                Status = "SUCCESS"
            };
            await _repository.SaveLogAsync(log);
        }

        // Act
        var logs = await _repository.GetLogsAsync(limit: 100);

        // Assert
        Assert.Equal(3, logs.Count);
    }

    [Fact]
    public async Task GetLogsWithFilterAsync_WithValidAction_ShouldReturnFilteredLogs()
    {
        // Arrange
        await _repository.SaveLogAsync(new LogEntry { Timestamp = DateTime.UtcNow, Action = "ENABLE", Status = "SUCCESS" });
        await _repository.SaveLogAsync(new LogEntry { Timestamp = DateTime.UtcNow, Action = "DISABLE", Status = "SUCCESS" });
        await _repository.SaveLogAsync(new LogEntry { Timestamp = DateTime.UtcNow, Action = "ENABLE", Status = "SUCCESS" });

        // Act
        var logs = await _repository.GetLogsWithFilterAsync("ENABLE");

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.All(logs, log => Assert.Equal("ENABLE", log.Action));
    }

    [Fact]
    public async Task GetLogsWithFilterAsync_WithNonExistentAction_ShouldReturnEmptyList()
    {
        // Arrange
        await _repository.SaveLogAsync(new LogEntry { Timestamp = DateTime.UtcNow, Action = "ENABLE", Status = "SUCCESS" });

        // Act
        var logs = await _repository.GetLogsWithFilterAsync("NONEXISTENT");

        // Assert
        Assert.Empty(logs);
    }

    #endregion

    #region Status Tests

    [Fact]
    public async Task GetCurrentStatusAsync_ShouldReturnDisabledByDefault()
    {
        // Act
        var status = await _repository.GetCurrentStatusAsync();

        // Assert
        Assert.Equal(BlockerStatus.Disabled, status);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidStatus_ShouldCreateLogEntry()
    {
        // Act
        await _repository.UpdateStatusAsync(BlockerStatus.Enabled);

        // Assert
        var logs = await _repository.GetLogsAsync();
        Assert.NotEmpty(logs);
        Assert.Contains("STATUS_UPDATE", logs[0].Action);
    }

    #endregion

    #region Custom Sites Tests

    [Fact]
    public async Task SaveCustomSiteAsync_WithValidSite_ShouldSaveSuccessfully()
    {
        // Arrange
        var site = new CustomBlockedSite
        {
            Domain = "example.com",
            AddedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        await _repository.SaveCustomSiteAsync(site);

        // Assert
        var sites = await _repository.GetCustomSitesAsync();
        Assert.Single(sites);
        Assert.Equal("example.com", sites[0].Domain);
    }

    [Fact]
    public async Task SaveCustomSiteAsync_WithNullSite_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveCustomSiteAsync(null));
    }

    [Fact]
    public async Task SaveCustomSiteAsync_WithDuplicateDomain_ShouldUpdateExisting()
    {
        // Arrange
        var site1 = new CustomBlockedSite { Domain = "example.com", AddedAt = DateTime.UtcNow };
        var site2 = new CustomBlockedSite { Domain = "example.com", AddedAt = DateTime.UtcNow.AddHours(1) };

        // Act
        await _repository.SaveCustomSiteAsync(site1);
        await _repository.SaveCustomSiteAsync(site2);

        // Assert
        var sites = await _repository.GetCustomSitesAsync();
        Assert.Single(sites);
    }

    [Fact]
    public async Task GetCustomSitesAsync_ShouldOnlyReturnActiveSites()
    {
        // Arrange
        await _repository.SaveCustomSiteAsync(new CustomBlockedSite { Domain = "active.com", AddedAt = DateTime.UtcNow });
        await _repository.SaveCustomSiteAsync(new CustomBlockedSite { Domain = "inactive.com", AddedAt = DateTime.UtcNow });
        await _repository.DeleteCustomSiteAsync("inactive.com");

        // Act
        var sites = await _repository.GetCustomSitesAsync();

        // Assert
        Assert.Single(sites);
        Assert.Equal("active.com", sites[0].Domain);
    }

    [Fact]
    public async Task DeleteCustomSiteAsync_WithValidDomain_ShouldSoftDeleteSite()
    {
        // Arrange
        var site = new CustomBlockedSite { Domain = "example.com", AddedAt = DateTime.UtcNow };
        await _repository.SaveCustomSiteAsync(site);

        // Act
        await _repository.DeleteCustomSiteAsync("example.com");

        // Assert
        var sites = await _repository.GetCustomSitesAsync();
        Assert.Empty(sites);
    }

    [Fact]
    public async Task DeleteCustomSiteAsync_WithNullDomain_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.DeleteCustomSiteAsync(null));
    }

    [Fact]
    public async Task DeleteCustomSiteAsync_WithNonExistentDomain_ShouldNotThrow()
    {
        // Act & Assert (should not throw)
        await _repository.DeleteCustomSiteAsync("nonexistent.com");
    }

    #endregion
}
