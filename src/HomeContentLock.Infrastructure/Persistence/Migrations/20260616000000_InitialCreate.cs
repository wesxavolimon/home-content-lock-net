using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeContentLock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blocker_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocker_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blocker_custom_sites",
                columns: table => new
                {
                    domain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    added_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocker_custom_sites", x => x.domain);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocker_custom_sites");

            migrationBuilder.DropTable(
                name: "blocker_logs");
        }
    }
}
