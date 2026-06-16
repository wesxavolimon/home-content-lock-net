using Microsoft.EntityFrameworkCore;
using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using Serilog;

namespace HomeContentLock.Infrastructure.Persistence;

/// <summary>
/// SQLite implementation of IBlockerRepository.
/// Handles all persistence operations for logs, configuration, and custom sites.
/// </summary>
public class SqliteBlockerRepository : IBlockerRepository
{
    private readonly BlockerDbContext _context;
    private readonly ILogger _logger;

    public SqliteBlockerRepository(BlockerDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<LogEntry>> GetLogsAsync(int limit = 100)
    {
        try
        {
            return await _context.LogEntries
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving logs");
            throw;
        }
    }

    public async Task<List<LogEntry>> GetLogsWithFilterAsync(string action, int limit = 100)
    {
        try
        {
            return await _context.LogEntries
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving filtered logs for action: {Action}", action);
            throw;
        }
    }

    public async Task SaveLogAsync(LogEntry log)
    {
        try
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                _context.LogEntries.Add(log);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.Information("Log entry saved: {Action} - {Status}", log.Action, log.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving log entry");
            throw;
        }
    }

    public async Task<BlockerStatus> GetCurrentStatusAsync()
    {
        try
        {
            // Try to get from database first
            var defaultStatus = BlockerStatus.Disabled;

            // For now, return default status
            // In future, this will read from a config table
            return await Task.FromResult(defaultStatus);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving current status");
            throw;
        }
    }

    public async Task UpdateStatusAsync(BlockerStatus status)
    {
        try
        {
            // Log the status update
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = $"STATUS_UPDATE_{status}",
                Status = "SUCCESS",
                Details = $"Status changed to {status}"
            };

            await SaveLogAsync(log);
            _logger.Information("Blocker status updated to: {Status}", status);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating status to {Status}", status);
            throw;
        }
    }

    public async Task<List<CustomBlockedSite>> GetCustomSitesAsync()
    {
        try
        {
            return await _context.CustomBlockedSites
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.AddedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving custom blocked sites");
            throw;
        }
    }

    public async Task SaveCustomSiteAsync(CustomBlockedSite site)
    {
        try
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var existing = await _context.CustomBlockedSites
                    .FirstOrDefaultAsync(s => s.Domain == site.Domain);

                if (existing != null)
                {
                    existing.IsActive = true;
                    existing.AddedAt = site.AddedAt;
                    _context.CustomBlockedSites.Update(existing);
                }
                else
                {
                    _context.CustomBlockedSites.Add(site);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.Information("Custom site saved: {Domain}", site.Domain);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving custom site: {Domain}", site?.Domain);
            throw;
        }
    }

    public async Task DeleteCustomSiteAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Domain cannot be null or empty", nameof(domain));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var site = await _context.CustomBlockedSites
                    .FirstOrDefaultAsync(s => s.Domain == domain);

                if (site != null)
                {
                    site.IsActive = false;
                    _context.CustomBlockedSites.Update(site);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.Information("Custom site deleted (soft): {Domain}", domain);
                }
                else
                {
                    _logger.Warning("Attempt to delete non-existent site: {Domain}", domain);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting custom site: {Domain}", domain);
            throw;
        }
    }
}
