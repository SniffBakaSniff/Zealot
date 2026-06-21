using CommunityToolkit.HighPerformance.Helpers;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zealot.Shared.Database;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Shared.Services
{
    public class GuildDataService(BotDbContext dbContext) : IGuildDataService
    {
        private readonly BotDbContext _dbContext = dbContext;
 
        // Adds the guild information to the database
        public async Task AddClientGuilds(DiscordGuild guild)
        {
            try
            {
                Log.Information("Adding Guild `{GuildName}` to database.", guild.Name);
                var guildData = await _dbContext.GuildsData.FindAsync(guild.Id);

                if (guildData is null)
                {
                    guildData = new GuildsData
                    {
                        GuildId = guild.Id,
                        Name = guild.Name,
                        IconUrl = guild.GetIconUrl(MediaFormat.Auto),
                        TotalMembers = guild.MemberCount,
                        Description = guild.Description,
                        CreatedAt = guild.CreationTimestamp.UtcDateTime,
                        Timestamp = DateTime.UtcNow
                    };
                    Log.Debug("Guild Data: {@guildData}", guildData);

                    await _dbContext.GuildsData.AddAsync(guildData);
                    await _dbContext.SaveChangesAsync();
                    return;
                }

                guildData.Name = guild.Name;
                guildData.IconUrl = guild.GetIconUrl(MediaFormat.Auto);
                guildData.TotalMembers = guild.MemberCount;
                guildData.Description = guild.Description;
                guildData.CreatedAt = guild.CreationTimestamp.UtcDateTime;
                guildData.Timestamp = DateTime.UtcNow;
                Log.Debug("Guild Data: {@guildData}", guildData);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Log.Error("An Exception during: `AddClientGuilds` Exception: {ex}", ex);
            }
        }

        // Removes the guild information from the database
        public async Task RemoveClientGuild(ulong guildId)
        {
            var existingGuild = await _dbContext.GuildsData.FindAsync(guildId);

            if (existingGuild is not null)
            {
                _dbContext.GuildsData.Remove(existingGuild);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Gets all guilds from the database
        public async Task<List<GuildsData>> GetAllGuildsAsync()
        {
            return await _dbContext.GuildsData.ToListAsync();
        }

        // Gets a specific guild by its ID
        public async Task<GuildsData?> GetGuildByIdAsync(ulong guildId)
        {
            return await _dbContext.GuildsData.FindAsync(guildId);
        }

        // Gets guilds by a list of IDs
        // Comapares the ID's of the users guilds and the bots guilds and returns the matching guilds from the database
        public async Task<List<GuildsData>> GetGuildsByIdsAsync(IEnumerable<ulong> guildIds)
        {
            return await _dbContext.GuildsData.Where(g => guildIds.Contains(g.GuildId)).ToListAsync();
        }
    }
}