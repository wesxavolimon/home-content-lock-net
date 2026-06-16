using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for retrieving the current blocker status and log count.
/// </summary>
public class GetStatusUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly ILogger _logger;

    public GetStatusUseCase(IBlockerRepository repository, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case.
    /// </summary>
    public async Task<Result<BlockerStatusDto>> ExecuteAsync()
    {
        try
        {
            var status = await _repository.GetCurrentStatusAsync();
            var logs = await _repository.GetLogsAsync(limit: 1000);

            var dto = new BlockerStatusDto
            {
                State = status.ToString().ToUpperInvariant(),
                LogCount = logs.Count,
                LastUpdated = DateTime.UtcNow
            };

            _logger.Information("Status retrieved: {State}, {LogCount} logs", dto.State, dto.LogCount);
            return Result.Success(dto, "Status retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving status");
            return Result.Failure<BlockerStatusDto>("Failed to retrieve status", ex);
        }
    }
}
