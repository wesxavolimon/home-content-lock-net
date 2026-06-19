using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;

namespace HomeContentLock.Application.UseCases.GetStatus;

public class GetStatusUseCase
{
    private readonly IBlockerRepository _repository;

    public GetStatusUseCase(IBlockerRepository repository)
    {
        _repository = repository;
    }

    public async Task<BlockerStatus> ExecuteAsync()
    {
        return await _repository.GetCurrentStatusAsync();
    }
}
