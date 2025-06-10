using Microsoft.EntityFrameworkCore;
using DSharpPlus.Entities;

using Zealot.Services.Interfaces;
using Zealot.Database.Models;
using Zealot.Databases;
using DSharpPlus;

namespace Zealot.Services
{
    public class ModerationLogService : IModerationLogService
    {
        private readonly DiscordClient _client;
        private readonly BotDbContext _dbContext;
        private readonly IGuildSettingService _guildSettingService;

        public ModerationLogService(DiscordClient client, BotDbContext dbContext, IGuildSettingService guildSettingService)
        {
            _client = client;
            _dbContext = dbContext;
            _guildSettingService = guildSettingService;
        }

        // Task responsible for handling the insertion of log items into the database
        public async Task LogModeratorActionAsync(
            ulong guildId,
            ulong? userId,
            ulong moderatorId,
            string actionType,
            string? reason = null,
            TimeSpan? duration = null,
            string? contextMessage = null,
            DateTimeOffset? expiresAt = null,
            DiscordEmbed? embed = null)
        {
            // create a log item 
            var log = new ModeratorLogs
            {
                GuildId = guildId,
                UserId = userId,
                ModeratorId = moderatorId,
                ActionType = actionType,
                Reason = reason,
                Duration = duration,
                ContextMessage = contextMessage,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
            };

            // Get the lase CaseNumber 
            var lastCase = await _dbContext.ModeratorLogs
                .Where(l => l.GuildId == log.GuildId)
                .OrderByDescending(l => l.CaseNumber)
                .Select(l => l.CaseNumber)
                .FirstOrDefaultAsync();

            // Increment the CaseNumber by 1
            log.CaseNumber = lastCase + 1;

            // Add the log to the database and save
            _dbContext.ModeratorLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            // Send the embed to the logging channel if the embed is not null
            if (embed is not null)
            {
                await SendEmbedToLogsChannel(guildId, embed);
            }
        }

        public async Task<IEnumerable<ModeratorLogsDTO>> GetModeratorLogsAsync(
            ulong? guildId = null,
            ulong? userId = null,
            ulong? moderatorId = null,
            string? actionType = null,
            int? caseNumber = null,
            DateTimeOffset? createdAfter = null,
            DateTimeOffset? createdBefore = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _dbContext.ModeratorLogs.AsQueryable();

            // Filter the query
            if (guildId.HasValue)
                query = query.Where(log => log.GuildId == guildId.Value);

            if (userId.HasValue)
                query = query.Where(log => log.UserId == userId.Value);

            if (moderatorId.HasValue)
                query = query.Where(log => log.ModeratorId == moderatorId.Value);

            if (!string.IsNullOrWhiteSpace(actionType))
                query = query.Where(log => log.ActionType == actionType);

            if (caseNumber.HasValue)
                query = query.Where(log => log.CaseNumber == caseNumber.Value);

            if (createdAfter.HasValue)
                query = query.Where(log => log.CreatedAt >= createdAfter.Value.ToUniversalTime());

            if (createdBefore.HasValue)
                query = query.Where(log => log.CreatedAt <= createdBefore.Value.ToUniversalTime());

            // Order and paginate the query
            query = query.OrderByDescending(log => log.CaseNumber);
            int skip = (page - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            // Send the query
            return await query
                .Select(log => new ModeratorLogsDTO
                {
                    UserId = log.UserId,
                    ModeratorId = log.ModeratorId,
                    ActionType = log.ActionType,
                    Reason = log.Reason,
                    CreatedAt = log.CreatedAt,
                    CaseNumber = log.CaseNumber
                })
                .ToListAsync();
        }

        /// <summary>
        /// Sends a Discord embed message to the configured moderation log channel for a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild where the message should be sent.</param>
        /// <param name="embed">The embed to send to the logging channel.</param>
        public async Task SendEmbedToLogsChannel(ulong guildId, DiscordEmbed embed)
        {
            // Get the moderation logging channel
            ulong? channelId = await _guildSettingService.GetModerationLogChannelAsync(guildId);

            // Perform a null check to make sure that the channel id exists
            if (channelId is null)
            {
                return; // Do nothing if no Moderation Logging Channel
            }

            // Get the discord channel
            var channel = await _client.GetChannelAsync(channelId.Value);

            // Send the embed to the Moderation Logging Channel
            await channel.SendMessageAsync(embed);
        }
    }
}
