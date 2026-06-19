using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;

namespace HomeContentLock.Application.UseCases.DisableBlocker;

public class DisableBlockerUseCase
{
    private readonly IBlockerRepository _repository;

    public DisableBlockerUseCase(IBlockerRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync()
    {
        await _repository.UpdateStatusAsync(BlockerStatus.Disabled);
        var log = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "DISABLE",
            Status = "SUCCESS",
            Details = "Blocker disabled"
        };
        await _repository.SaveLogAsync(log);
    }
}
