using HomeContentLock.Domain.Entities;

namespace HomeContentLock.Domain.Interfaces;

/// <summary>
/// Repository interface for blocker operations.
/// Handles persistence of logs, configuration, and custom blocked sites.
/// </summary>
public interface IBlockerRepository
{
    /// <summary>
    /// Retrieve logs with a default limit.
    /// </summary>
    Task<List<LogEntry>> GetLogsAsync(int limit = 100);

    /// <summary>
    /// Retrieve logs filtered by action type.
    /// </summary>
    Task<List<LogEntry>> GetLogsWithFilterAsync(string action, int limit = 100);

    /// <summary>
    /// Save a log entry to the database.
    /// </summary>
    Task SaveLogAsync(LogEntry log);

    /// <summary>
    /// Get the current blocker status from configuration.
    /// </summary>
    Task<BlockerStatus> GetCurrentStatusAsync();

    /// <summary>
    /// Update the blocker status in configuration.
    /// </summary>
    Task UpdateStatusAsync(BlockerStatus status);

    /// <summary>
    /// Retrieve all active custom blocked sites.
    /// </summary>
    Task<List<CustomBlockedSite>> GetCustomSitesAsync();

    /// <summary>
    /// Save a custom blocked site.
    /// </summary>
    Task SaveCustomSiteAsync(CustomBlockedSite site);

    /// <summary>
    /// Delete a custom blocked site (soft delete).
    /// </summary>
    Task DeleteCustomSiteAsync(string domain);
}
