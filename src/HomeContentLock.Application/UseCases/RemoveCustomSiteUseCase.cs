using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for removing a custom site from the block list.
/// </summary>
public class RemoveCustomSiteUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly ILogger _logger;

    public RemoveCustomSiteUseCase(IBlockerRepository repository, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case.
    /// </summary>
    public async Task<Result> ExecuteAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.Warning("Remove custom site attempted with empty domain");
                return Result.Failure("Domain cannot be empty");
            }

            domain = domain.ToLowerInvariant().Trim();

            await _repository.DeleteCustomSiteAsync(domain);

            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "SITE_REMOVED",
                Status = "SUCCESS",
                Details = $"Custom site removed: {domain}"
            };

            await _repository.SaveLogAsync(log);

            _logger.Information("Custom site removed: {Domain}", domain);
            return Result.Success($"Site '{domain}' removed from block list");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error removing custom site");
            return Result.Failure("Failed to remove custom site", ex);
        }
    }
}
