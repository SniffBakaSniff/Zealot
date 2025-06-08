using Microsoft.EntityFrameworkCore;

using Zealot.Services.Interfaces;
using Zealot.Database.Models;
using Zealot.Databases;

namespace Zealot.Services
{
    public class GuildSettingService : IGuildSettingService
    {
        // Define the Database Context
        private readonly BotDbContext _dbContext;

        public GuildSettingService(BotDbContext botDbContext)
        {
            _dbContext = botDbContext;
        }

        // Task to get the guilds prefix from the database
        public async Task<string> GetGuildPrefixAsync(ulong guildId)
        {
            // Get the GuildSettings table for the guild
            var settings = await _dbContext.GuildSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            // returns the prefix located in the database or `!` if null
            return settings?.Prefix ?? "!";
        }

        // Task to set the guilds prefix in the database
        public async Task SetGuildPrefixAsync(ulong guildId, string prefix)
        {
            // Try to get the current settings for the guild
            var settings = await _dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            if (settings == null)
            {
                // Create a new record if none exists
                settings = new GuildSettings
                {
                    GuildId = guildId,
                    Prefix = prefix
                };

                await _dbContext.GuildSettings.AddAsync(settings);
            }
            else
            {
                // Update the existing prefix
                settings.Prefix = prefix;

                _dbContext.GuildSettings.Update(settings);
            }

            // Save the changes
            await _dbContext.SaveChangesAsync();
        }

    }
}