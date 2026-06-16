using Microsoft.EntityFrameworkCore;
using HomeContentLock.Domain.Entities;

namespace HomeContentLock.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for HomeContentLock.
/// Manages persistence of logs, configuration, and custom blocked sites.
/// </summary>
public class BlockerDbContext : DbContext
{
    public BlockerDbContext(DbContextOptions<BlockerDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<CustomBlockedSite> CustomBlockedSites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LogEntry
        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Timestamp)
                .IsRequired();

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Details)
                .HasMaxLength(2000);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.ToTable("blocker_logs");
        });

        // Configure CustomBlockedSite
        modelBuilder.Entity<CustomBlockedSite>(entity =>
        {
            entity.HasKey(e => e.Domain);

            entity.Property(e => e.Domain)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.AddedAt)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.ToTable("blocker_custom_sites");
        });
    }
}
