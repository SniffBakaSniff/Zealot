using Microsoft.EntityFrameworkCore;

using Zealot.Shared.Services.Interfaces;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Database;

namespace Zealot.Shared.Services
{
    public class GuildSettingService(BotDbContext dbContext) : IGuildSettingService
    {
        // Define the Database Context

        #region GetGuildPrefixAsync
        // Task to get the guilds prefix from the database
        public async Task<string> GetGuildPrefixAsync(ulong guildId)
        {
            // Get the GuildSettings table for the guild
            var settings = await dbContext.GuildSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            // returns the prefix located in the database or `!` if null
            return settings?.Prefix ?? "!";
        }
        #endregion

        #region SetGuildPrefixAsync
        // Task to set the guilds prefix in the database
        public async Task SetGuildPrefixAsync(ulong guildId, string prefix)
        {
            // Try to get the current settings for the guild
            var settings = await dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            if (settings == null)
            {
                // Create a new record if none exists
                settings = new GuildSettings
                {
                    GuildId = guildId,
                    Prefix = prefix
                };

                await dbContext.GuildSettings.AddAsync(settings);
            }
            else
            {
                // Update the existing prefix
                settings.Prefix = prefix;

                dbContext.GuildSettings.Update(settings);
            }

            // Save the changes
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region GetModerationLogChannelAsync
        public async Task<ulong?> GetModerationLogChannelAsync(ulong guildId)
        {
            var settings = await dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            return settings?.ModerationLogChannel;
        }
        #endregion

        #region SetModerationLogChannelAsync
        // Task to set the ModerationLogChannel in the database
        public async Task SetModerationLogChannelAsync(ulong guildId, ulong channelId)
        {
            // Try to get the current settings for the guild
            var settings = await dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            if (settings == null)
            {
                // Create a new record if none exists
                settings = new GuildSettings
                {
                    GuildId = guildId,
                    ModerationLogChannel = channelId
                };

                await dbContext.GuildSettings.AddAsync(settings);
            }
            else
            {
                // Update the existing ModerationLoggingChannel
                settings.ModerationLogChannel = channelId;

                dbContext.GuildSettings.Update(settings);
            }

            // Save the changes to the database
            await dbContext.SaveChangesAsync();
        }
        #endregion
    }
}