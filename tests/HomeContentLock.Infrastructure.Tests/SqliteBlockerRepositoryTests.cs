using Xunit;
using HomeContentLock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeContentLock.Infrastructure.Tests;

public class SqliteBlockerRepositoryTests : IAsyncLifetime
{
    private BlockerDatabase _db;
    private SqliteBlockerRepository _repository;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BlockerDatabase>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _db = new BlockerDatabase(options);
        await _db.Database.EnsureCreatedAsync();
        _repository = new SqliteBlockerRepository(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.Database.EnsureDeletedAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SaveLogAsync_PersistsLog()
    {
        var log = new LogEntry 
        { 
            Timestamp = DateTime.UtcNow, 
            Action = "TEST", 
            Status = "SUCCESS" 
        };

        await _repository.SaveLogAsync(log);
        var logs = await _repository.GetLogsAsync();

        Assert.Single(logs);
    }
}
