using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.UseCases;
using Moq;
using Serilog;

namespace HomeContentLock.Application.Tests.UseCases;

public class AddCustomSiteUseCaseTests
{
    private readonly Mock<IBlockerRepository> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly AddCustomSiteUseCase _useCase;

    public AddCustomSiteUseCaseTests()
    {
        _repositoryMock = new Mock<IBlockerRepository>();
        _loggerMock = new Mock<ILogger>();
        _useCase = new AddCustomSiteUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidDomain_ShouldSucceed()
    {
        // Arrange
        const string domain = "example.com";

        // Act
        var result = await _useCase.ExecuteAsync(domain);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveCustomSiteAsync(It.Is<Domain.Entities.CustomBlockedSite>(
            s => s.Domain == domain)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDomain_ShouldFail()
    {
        // Act
        var result = await _useCase.ExecuteAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveCustomSiteAsync(It.IsAny<Domain.Entities.CustomBlockedSite>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDomain_ShouldFail()
    {
        // Act
        var result = await _useCase.ExecuteAsync(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveCustomSiteAsync(It.IsAny<Domain.Entities.CustomBlockedSite>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDomain_ShouldFail()
    {
        // Act
        var result = await _useCase.ExecuteAsync("invalid domain with spaces");

        // Assert
        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveCustomSiteAsync(It.IsAny<Domain.Entities.CustomBlockedSite>()), Times.Never);
    }

    [Theory]
    [InlineData("google.com")]
    [InlineData("example.co.uk")]
    [InlineData("*.example.com")]
    public async Task ExecuteAsync_WithValidDomainVariations_ShouldSucceed(string domain)
    {
        // Act
        var result = await _useCase.ExecuteAsync(domain);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNormalizeToLowerCase()
    {
        // Arrange
        const string domain = "EXAMPLE.COM";

        // Act
        var result = await _useCase.ExecuteAsync(domain);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.SaveCustomSiteAsync(It.Is<Domain.Entities.CustomBlockedSite>(
            s => s.Domain == domain.ToLowerInvariant())), Times.Once);
    }
}
