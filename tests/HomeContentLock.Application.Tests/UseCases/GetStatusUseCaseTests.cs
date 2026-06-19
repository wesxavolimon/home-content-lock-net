using Xunit;
using Moq;
using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.UseCases.GetStatus;

namespace HomeContentLock.Application.Tests.UseCases;

public class GetStatusUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsCurrentStatus()
    {
        var mockRepository = new Mock<IBlockerRepository>();
        mockRepository.Setup(r => r.GetCurrentStatusAsync())
            .ReturnsAsync(BlockerStatus.Enabled);

        var useCase = new GetStatusUseCase(mockRepository.Object);
        var result = await useCase.ExecuteAsync();

        Assert.Equal(BlockerStatus.Enabled, result);
    }
}
