using Xunit;
using Moq;
using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.UseCases.EnableBlocker;

namespace HomeContentLock.Application.Tests.UseCases;

public class EnableBlockerUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesStatusAndLogsAction()
    {
        var mockRepository = new Mock<IBlockerRepository>();

        var useCase = new EnableBlockerUseCase(mockRepository.Object);
        await useCase.ExecuteAsync();

        mockRepository.Verify(r => r.UpdateStatusAsync(BlockerStatus.Enabled), Times.Once);
        mockRepository.Verify(r => r.SaveLogAsync(It.IsAny<LogEntry>()), Times.Once);
    }
}
