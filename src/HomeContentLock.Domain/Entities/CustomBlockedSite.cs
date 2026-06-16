namespace HomeContentLock.Domain.Entities;

public class CustomBlockedSite
{
    public string Domain { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}