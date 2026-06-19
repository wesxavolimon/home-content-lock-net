using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;

namespace HomeContentLock.Application.UseCases.QueryLogs;

public class QueryLogsUseCase
{
    private readonly IBlockerRepository _repository;

    public QueryLogsUseCase(IBlockerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LogEntry>> ExecuteAsync(int limit = 100)
    {
        return await _repository.GetLogsAsync(limit);
    }
}
