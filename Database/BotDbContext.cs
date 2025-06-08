using Microsoft.EntityFrameworkCore;

using Zealot.Database.Models;

namespace Zealot.Databases
{
    /// <summary>
    /// Database context for the CountingBot application.
    /// </summary>
    public class BotDbContext : DbContext
    {
        public BotDbContext() { }

        public DbSet<GuildSettings> GuildSettings { get; set; }
        public DbSet<ModeratorLogs> ModeratorLogs { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if no options have been passed in (e.g., by dependency injection)
            if (!optionsBuilder.IsConfigured)
            {
                // Can use my VPS's database server for this
                optionsBuilder.UseNpgsql(
                  "Host=localhost;Database=dev;Username=Subaka;Password=Subaka1@;Maximum Pool Size=128;Minimum Pool Size=5;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add entity configuration here if needed
        }
    }
}
