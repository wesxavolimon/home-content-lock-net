using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Exceptions;
using HomeContentLock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeContentLock.Infrastructure.Persistence;

public class SqliteBlockerRepository : IBlockerRepository
{
    private readonly BlockerDatabase _db;

    public SqliteBlockerRepository(BlockerDatabase db)
    {
        _db = db;
    }

    public async Task<List<LogEntry>> GetLogsAsync(int limit = 100)
    {
        return await _db.Logs.OrderByDescending(l => l.Timestamp).Take(limit).ToListAsync();
    }

    public async Task<List<LogEntry>> GetLogsWithFilterAsync(string action, int limit = 100)
    {
        return await _db.Logs.Where(l => l.Action == action).OrderByDescending(l => l.Timestamp).Take(limit).ToListAsync();
    }

    public async Task SaveLogAsync(LogEntry log)
    {
        try
        {
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Failed to save log entry", ex);
        }
    }

    public async Task<BlockerStatus> GetCurrentStatusAsync()
    {
        var config = await _db.Database.SqlQueryRaw<dynamic>("SELECT value FROM blocker_config WHERE key = 'current_status' LIMIT 1").ToListAsync();
        if (config.Count == 0) return BlockerStatus.Disabled;
        var statusStr = (string)config[0].value;
        return Enum.TryParse<BlockerStatus>(statusStr, out var status) ? status : BlockerStatus.Disabled;
    }

    public async Task UpdateStatusAsync(BlockerStatus status)
    {
        try
        {
            await _db.Database.ExecuteSqlAsync($"UPDATE blocker_config SET value = '{status}' WHERE key = 'current_status'");
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Failed to update status", ex);
        }
    }

    public async Task<List<CustomBlockedSite>> GetCustomSitesAsync()
    {
        return await _db.CustomSites.Where(s => s.IsActive).ToListAsync();
    }

    public async Task SaveCustomSiteAsync(CustomBlockedSite site)
    {
        try
        {
            _db.CustomSites.Add(site);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Failed to save custom site", ex);
        }
    }

    public async Task DeleteCustomSiteAsync(string domain)
    {
        try
        {
            var site = await _db.CustomSites.FindAsync(domain);
            if (site != null)
            {
                site.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Failed to delete custom site", ex);
        }
    }
}
