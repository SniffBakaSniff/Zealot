using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Zealot.Database.Models;

namespace Zealot.Database
{
    /// <summary>
    /// Database context for the CountingBot application.
    /// </summary>
    public class BotDbContext : DbContext
    {
        public BotDbContext() { }

        public DbSet<GuildSettings> GuildSettings { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if no options have been passed in (e.g., by dependency injection)
            if (!optionsBuilder.IsConfigured)
            {
                // Can use my VPS's database server for this
                optionsBuilder.UseNpgsql(
                  "Host=VPS-IP;Database=CountingBotDb;Username=subaka;Password=Subaka1@;Maximum Pool Size=128;Minimum Pool Size=5;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add entity configuration here if needed
        }
    }
}
