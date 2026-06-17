using HomeContentLock.Domain.Entities;

namespace HomeContentLock.Domain.Interfaces;

public interface IBlockerRepository
{
    Task<List<LogEntry>> GetLogsAsync(int limit = 100);
    Task<List<LogEntry>> GetLogsWithFilterAsync(string action, int limit = 100);
    Task SaveLogAsync(LogEntry log);
    Task<BlockerStatus> GetCurrentStatusAsync();
    Task UpdateStatusAsync(BlockerStatus status);
    Task<List<CustomBlockedSite>> GetCustomSitesAsync();
    Task SaveCustomSiteAsync(CustomBlockedSite site);
    Task DeleteCustomSiteAsync(string domain);
}
