using HomeContentLock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeContentLock.Infrastructure.Persistence;

public class BlockerDatabase : DbContext
{
    public BlockerDatabase(DbContextOptions<BlockerDatabase> options) : base(options) { }

    public DbSet<LogEntry> Logs { get; set; }
    public DbSet<CustomBlockedSite> CustomSites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Details).HasMaxLength(1000);
        });

        modelBuilder.Entity<CustomBlockedSite>(entity =>
        {
            entity.HasKey(e => e.Domain);
            entity.Property(e => e.Domain).HasMaxLength(255);
        });
    }
}
