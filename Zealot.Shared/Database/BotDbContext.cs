using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Zealot.Shared.Database.Models;

namespace Zealot.Shared.Database
{
    /// <summary>
    /// Database context for Zealot.
    /// </summary>
    public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options)
    {
        public DbSet<GuildSettings> GuildSettings { get; set; }
        public DbSet<WarningData> WarningData { get; set; }
        public DbSet<ModeratorLogs> ModeratorLogs { get; set; }
        public DbSet<ScheduledTasks> ScheduledTasks { get; set; }
        public DbSet<GuildsData> GuildsData { get; set; }
        public DbSet<WarningEscalationRule> WarningEscalationRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevents the creation of multiple rules using the same warning count
            modelBuilder.Entity<WarningEscalationRule>().HasIndex(x => new
            {
                x.GuildId,
                x.WarningCount
            })
            .IsUnique();
        }
    }

    public sealed class BotDataSource(NpgsqlDataSource dataSource)
    {
        public NpgsqlDataSource DataSource { get; } = dataSource;
    }
}
