using HomeContentLock.Application.Results;
using HomeContentLock.Application.UseCases;
using Serilog;

namespace HomeContentLock.Application.Services;

/// <summary>
/// Application service that orchestrates all blocker use cases.
/// Provides a single entry point for all business logic operations.
/// </summary>
public class BlockerService
{
    private readonly GetStatusUseCase _getStatusUseCase;
    private readonly EnableBlockerUseCase _enableBlockerUseCase;
    private readonly DisableBlockerUseCase _disableBlockerUseCase;
    private readonly QueryLogsUseCase _queryLogsUseCase;
    private readonly SetPasswordUseCase _setPasswordUseCase;
    private readonly AddCustomSiteUseCase _addCustomSiteUseCase;
    private readonly RemoveCustomSiteUseCase _removeCustomSiteUseCase;
    private readonly ILogger _logger;

    public BlockerService(
        GetStatusUseCase getStatusUseCase,
        EnableBlockerUseCase enableBlockerUseCase,
        DisableBlockerUseCase disableBlockerUseCase,
        QueryLogsUseCase queryLogsUseCase,
        SetPasswordUseCase setPasswordUseCase,
        AddCustomSiteUseCase addCustomSiteUseCase,
        RemoveCustomSiteUseCase removeCustomSiteUseCase,
        ILogger logger)
    {
        _getStatusUseCase = getStatusUseCase ?? throw new ArgumentNullException(nameof(getStatusUseCase));
        _enableBlockerUseCase = enableBlockerUseCase ?? throw new ArgumentNullException(nameof(enableBlockerUseCase));
        _disableBlockerUseCase = disableBlockerUseCase ?? throw new ArgumentNullException(nameof(disableBlockerUseCase));
        _queryLogsUseCase = queryLogsUseCase ?? throw new ArgumentNullException(nameof(queryLogsUseCase));
        _setPasswordUseCase = setPasswordUseCase ?? throw new ArgumentNullException(nameof(setPasswordUseCase));
        _addCustomSiteUseCase = addCustomSiteUseCase ?? throw new ArgumentNullException(nameof(addCustomSiteUseCase));
        _removeCustomSiteUseCase = removeCustomSiteUseCase ?? throw new ArgumentNullException(nameof(removeCustomSiteUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get the current status of the blocker.
    /// </summary>
    public async Task<Result<BlockerStatusDto>> GetStatusAsync()
    {
        _logger.Debug("BlockerService: GetStatus requested");
        return await _getStatusUseCase.ExecuteAsync();
    }

    /// <summary>
    /// Enable the content blocker.
    /// </summary>
    public async Task<Result> EnableAsync(string password)
    {
        _logger.Debug("BlockerService: Enable requested");
        return await _enableBlockerUseCase.ExecuteAsync(password);
    }

    /// <summary>
    /// Disable the content blocker.
    /// </summary>
    public async Task<Result> DisableAsync(string password)
    {
        _logger.Debug("BlockerService: Disable requested");
        return await _disableBlockerUseCase.ExecuteAsync(password);
    }

    /// <summary>
    /// Query logs with optional filtering.
    /// </summary>
    public async Task<Result<List<LogEntryDto>>> QueryLogsAsync(string? action = null, int limit = 100)
    {
        _logger.Debug("BlockerService: QueryLogs requested with action={Action}, limit={Limit}", action, limit);
        return await _queryLogsUseCase.ExecuteAsync(action, limit);
    }

    /// <summary>
    /// Set or update the password.
    /// </summary>
    public async Task<Result> SetPasswordAsync(string newPassword)
    {
        _logger.Debug("BlockerService: SetPassword requested");
        return await _setPasswordUseCase.ExecuteAsync(newPassword);
    }

    /// <summary>
    /// Add a custom site to the block list.
    /// </summary>
    public async Task<Result> AddCustomSiteAsync(string domain)
    {
        _logger.Debug("BlockerService: AddCustomSite requested for domain={Domain}", domain);
        return await _addCustomSiteUseCase.ExecuteAsync(domain);
    }

    /// <summary>
    /// Remove a custom site from the block list.
    /// </summary>
    public async Task<Result> RemoveCustomSiteAsync(string domain)
    {
        _logger.Debug("BlockerService: RemoveCustomSite requested for domain={Domain}", domain);
        return await _removeCustomSiteUseCase.ExecuteAsync(domain);
    }
}
