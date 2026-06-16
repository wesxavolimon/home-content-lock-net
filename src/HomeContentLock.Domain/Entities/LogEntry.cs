namespace HomeContentLock.Domain.Entities;

public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty; // ENABLE, DISABLE, SITE_ADDED, etc
    public string Status { get; set; } = string.Empty; // SUCCESS, FAILED, PENDING
    public string Details { get; set; } = string.Empty; // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}