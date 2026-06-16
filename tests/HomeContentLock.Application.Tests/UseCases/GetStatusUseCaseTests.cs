using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.UseCases;
using Moq;
using Serilog;

namespace HomeContentLock.Application.Tests.UseCases;

public class GetStatusUseCaseTests
{
    private readonly Mock<IBlockerRepository> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly GetStatusUseCase _useCase;

    public GetStatusUseCaseTests()
    {
        _repositoryMock = new Mock<IBlockerRepository>();
        _loggerMock = new Mock<ILogger>();
        _useCase = new GetStatusUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStatus()
    {
        // Arrange
        var logs = new List<LogEntry>
        {
            new LogEntry { Id = 1, Timestamp = DateTime.UtcNow, Action = "TEST", Status = "SUCCESS" }
        };

        _repositoryMock.Setup(r => r.GetCurrentStatusAsync()).ReturnsAsync(BlockerStatus.Disabled);
        _repositoryMock.Setup(r => r.GetLogsAsync(It.IsAny<int>())).ReturnsAsync(logs);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("DISABLED", result.Data.State);
        Assert.Equal(1, result.Data.LogCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoLogs_ShouldReturnZeroLogCount()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCurrentStatusAsync()).ReturnsAsync(BlockerStatus.Enabled);
        _repositoryMock.Setup(r => r.GetLogsAsync(It.IsAny<int>())).ReturnsAsync(new List<LogEntry>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data!.LogCount);
        Assert.Equal("ENABLED", result.Data.State);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCurrentStatusAsync()).ReturnsAsync(BlockerStatus.Disabled);
        _repositoryMock.Setup(r => r.GetLogsAsync(It.IsAny<int>())).ReturnsAsync(new List<LogEntry>());

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _repositoryMock.Verify(r => r.GetCurrentStatusAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetLogsAsync(It.IsAny<int>()), Times.Once);
    }
}
