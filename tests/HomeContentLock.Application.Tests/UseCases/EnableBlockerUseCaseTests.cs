using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Domain.Exceptions;
using HomeContentLock.Application.UseCases;
using Moq;
using Serilog;

namespace HomeContentLock.Application.Tests.UseCases;

public class EnableBlockerUseCaseTests
{
    private readonly Mock<IBlockerRepository> _repositoryMock;
    private readonly Mock<IPasswordValidator> _validatorMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly EnableBlockerUseCase _useCase;

    public EnableBlockerUseCaseTests()
    {
        _repositoryMock = new Mock<IBlockerRepository>();
        _validatorMock = new Mock<IPasswordValidator>();
        _loggerMock = new Mock<ILogger>();
        _useCase = new EnableBlockerUseCase(_repositoryMock.Object, _validatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPassword_ShouldSucceed()
    {
        // Arrange
        const string password = "test123";
        _validatorMock.Setup(v => v.ValidateAsync(password)).ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(password);

        // Assert
        Assert.True(result.IsSuccess);
        _validatorMock.Verify(v => v.ValidateAsync(password), Times.Once);
        _repositoryMock.Verify(r => r.SaveLogAsync(It.IsAny<LogEntry>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateStatusAsync(BlockerStatus.Enabled), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        const string password = "wrongpassword";
        _validatorMock.Setup(v => v.ValidateAsync(password)).ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(password);

        // Assert
        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveLogAsync(It.IsAny<LogEntry>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullPassword_ShouldFail()
    {
        // Act
        var result = await _useCase.ExecuteAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(v => v.ValidateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyPassword_ShouldFail()
    {
        // Act
        var result = await _useCase.ExecuteAsync(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(v => v.ValidateAsync(It.IsAny<string>()), Times.Never);
    }
}
