using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for querying blocker logs with optional filtering.
/// </summary>
public class QueryLogsUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly ILogger _logger;

    public QueryLogsUseCase(IBlockerRepository repository, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case with optional filter.
    /// </summary>
    public async Task<Result<List<LogEntryDto>>> ExecuteAsync(string? action = null, int limit = 100)
    {
        try
        {
            if (limit <= 0 || limit > 1000)
                limit = 100;

            List<LogEntry> logs;

            if (string.IsNullOrWhiteSpace(action))
            {
                logs = await _repository.GetLogsAsync(limit);
                _logger.Information("Retrieved {Count} logs", logs.Count);
            }
            else
            {
                logs = await _repository.GetLogsWithFilterAsync(action, limit);
                _logger.Information("Retrieved {Count} logs filtered by action: {Action}", logs.Count, action);
            }

            var dtos = logs.Select(l => new LogEntryDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                Action = l.Action,
                Status = l.Status,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            }).ToList();

            return Result.Success(dtos, $"Retrieved {dtos.Count} log entries");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error querying logs");
            return Result.Failure<List<LogEntryDto>>("Failed to query logs", ex);
        }
    }
}

/// <summary>
/// DTO for log entry
/// </summary>
public class LogEntryDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
