using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using HomeContentLock.Infrastructure.Persistence;

#nullable disable

namespace HomeContentLock.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(BlockerDbContext))]
    partial class BlockerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.9");

            modelBuilder.Entity("HomeContentLock.Domain.Entities.CustomBlockedSite", b =>
                {
                    b.Property<string>("Domain")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.HasKey("Domain");

                    b.ToTable("blocker_custom_sites", (string)null);
                });

            modelBuilder.Entity("HomeContentLock.Domain.Entities.LogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Details")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("blocker_logs", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
