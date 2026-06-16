namespace HomeContentLock.Domain.Entities;

public class ActivationSecret
{
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Algorithm { get; set; } = "SHA256";
}