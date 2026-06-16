using Microsoft.EntityFrameworkCore;
using HomeContentLock.Infrastructure.Persistence;
using Serilog;

namespace HomeContentLock.Infrastructure.Tests;

/// <summary>
/// Test fixture for managing test database lifecycle.
/// Uses in-memory SQLite for fast and isolated testing.
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    private readonly string _dbPath;
    public BlockerDbContext Context { get; private set; } = null!;
    public ILogger Logger { get; private set; }

    public TestDatabaseFixture()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_blocker_{Guid.NewGuid()}.db");

        // Configure Serilog for tests
        Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BlockerDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        Context = new BlockerDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }

        (Logger as IDisposable)?.Dispose();
    }
}

/// <summary>
/// Collection definition for database fixture sharing across tests
/// </summary>
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}
