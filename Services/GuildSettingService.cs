using Microsoft.EntityFrameworkCore;

using Zealot.Services.Interfaces;
using Zealot.Database.Models;
using Zealot.Databases;
using System.Runtime.CompilerServices;

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

        public async Task<ulong?> GetModerationLogChannelAsync(ulong guildId)
        {
            var settings = await _dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            return settings?.ModerationLogChannel;
        }

        // Task to set the ModerationLogChannel in the database
        public async Task SetModerationLogChannelAsync(ulong guildId, ulong channelId)
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
                    ModerationLogChannel = channelId
                };

                await _dbContext.GuildSettings.AddAsync(settings);
            }
            else
            {
                // Update the existing ModerationLoggingChannel
                settings.ModerationLogChannel = channelId;

                _dbContext.GuildSettings.Update(settings);
            }

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
        }

        // Task to set the MutedRoleId in the database
        public async Task SetMutedRoleIdAsync(ulong guildId, ulong roleId)
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
                    MutedRoleId = roleId
                };

                await _dbContext.GuildSettings.AddAsync(settings);
            }
            else
            {
                // Update the existing MutedRoleId
                settings.MutedRoleId = roleId;

                _dbContext.GuildSettings.Update(settings);
            }

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
        }

        // Task to to get the MutedRoleId from the database
        public async Task<ulong?> GetMutedRoleIdAsync(ulong guildId)
        {
            var settings = await _dbContext.GuildSettings
                .FirstOrDefaultAsync(s => s.GuildId == guildId);

            return settings?.MutedRoleId;
        }
    }
}