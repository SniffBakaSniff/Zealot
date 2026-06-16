using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Zealot.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    ModerationLogChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MutedRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ModeratorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ModeratorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Image = table.Column<byte[]>(type: "bytea", nullable: true),
                    CaseNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeratorLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    ExecuteAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTasks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildSettings");

            migrationBuilder.DropTable(
                name: "ModeratorLogs");

            migrationBuilder.DropTable(
                name: "ScheduledTasks");
        }
    }
}
