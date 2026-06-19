using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;

namespace HomeContentLock.Application.UseCases.EnableBlocker;

public class EnableBlockerUseCase
{
    private readonly IBlockerRepository _repository;

    public EnableBlockerUseCase(IBlockerRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync()
    {
        await _repository.UpdateStatusAsync(BlockerStatus.Enabled);
        var log = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "ENABLE",
            Status = "SUCCESS",
            Details = "Blocker enabled"
        };
        await _repository.SaveLogAsync(log);
    }
}
