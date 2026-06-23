using Microsoft.EntityFrameworkCore;

using Zealot.Shared.Services.Interfaces;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Database;
using System.Security.Cryptography.X509Certificates;
using Zealot.Shared.Enums;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Zealot.Shared.Services
{

    // Consider finding shorter Task names.
    public class WarningService(BotDbContext dbContext, DiscordClient client, ModerationLogService moderationLogService, TaskSchedulerService taskSchedulerService) : IWarningService
    {
        #region AddWarningAsync
        // This method adds a warning to the database for a specific user in a guild. 
        public async Task AddWarningAsync(ulong guildId, ulong userId, ulong moderatorId, string? reason = null)
        {
            if (reason?.Length > 2000)
            {
                throw new ArgumentException("Reason cannot exceed 2000 characters.");
            }

            var warning = new WarningData
            {
                GuildId = guildId,
                UserId = userId,
                ModeratorId = moderatorId,
                Reason = reason
            };

            int nextId = (await dbContext.WarningData
                .Where(x => x.GuildId == guildId)
                .MaxAsync(x => (int?)x.WarningId) ?? 0) + 1;

            warning.WarningId = nextId;

            await dbContext.WarningData.AddAsync(warning);
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region GetWarningsForGuildAsync
        // This Method retrieves all warnings for a guild.
        public async Task<IEnumerable<WarningData>> GetWarningsForGuildAsync(ulong guildId)
        {
            return await dbContext.WarningData.Where(w => w.GuildId == guildId).ToListAsync();
        }
        #endregion

        #region GetPaginatedWarningsForGuildAsync
        // This Method retrieves a paginated list of warnings for a guild.
        public async Task<IEnumerable<WarningData>> GetPaginatedWarningsForGuildAsync(ulong guildId, int page = 1, int pageSize = 20)
        {
            return await dbContext.WarningData
                .Where(w => w.GuildId == guildId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        #endregion

        #region GetWarningsForUserAsync
        // This method retrieves all warnings for a specific user in a guild.
        public async Task<IEnumerable<WarningData>> GetWarningsForUserAsync(ulong guildId, ulong userId)
        {
            return await dbContext.WarningData.Where(w => w.GuildId == guildId && w.UserId == userId).ToListAsync();
        }
        #endregion

        #region GetPaginatedWarningsForUserAsync
        // This method retrieves a paginated list of warnings for a specific user in a guild.
        public async Task<IEnumerable<WarningData>> GetPaginatedWarningsForUserAsync(ulong guildId, ulong userId, int page = 1, int pageSize = 20)
        {
            return await dbContext.WarningData
                .Where(w => w.GuildId == guildId && w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        #endregion

        #region GetWarningsByIdAsync
        // This method retrieves a specific warning by its ID for a user in a guild.
        public async Task<WarningData?> GetWarningByIdAsync(ulong guildId, int warningId)
        {
            return await dbContext.WarningData.Where(w => w.GuildId == guildId && w.WarningId == warningId).FirstOrDefaultAsync();
        }
        #endregion

        #region GetWarningCountAsync
        // This method retrieves the total count of warnings for a specific user in a guild.
        public async Task<int> GetWarningCountAsync(ulong guildId, ulong userId)
        {
            return await dbContext.WarningData
                .CountAsync(w => w.GuildId == guildId && w.UserId == userId);
        }
        #endregion

        #region ClearWarningsForUserAsync
        // This method clears all warnings for a specific user in a guild.
        public async Task ClearWarningsForUserAsync(ulong guildId, ulong userId)
        {
            var warnings = await dbContext.WarningData.Where(w => w.GuildId == guildId && w.UserId == userId).ToListAsync();
            dbContext.WarningData.RemoveRange(warnings);
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region ClearWarningByIdAsync
        // This method clears a specific warning by its ID.
        public async Task ClearWarningByIdAsync(ulong guildId, int warningId)
        {
            var warning = await dbContext.WarningData.Where(w => w.GuildId == guildId && w.WarningId == warningId).FirstOrDefaultAsync();
            if (warning != null)
            {
                dbContext.WarningData.Remove(warning);
                await dbContext.SaveChangesAsync();
            }
        }
        #endregion

        #region Warningescalation Async
        // Might move this to a ModeratorActionService or something.
        // This meathod will handle warning escalation .
        public async Task WarningescalationAsync(ulong guildId, ulong userId)
        {
            DiscordGuild guild = await client.GetGuildAsync(guildId);
            DiscordMember user = await guild.GetMemberAsync(userId);
            DiscordUser? moderator = client.CurrentApplication.Bot;
            int warningCount = await GetWarningCountAsync(guildId, userId);
            var escalationRule = await dbContext.WarningEscalationRules
                                .Where(w => w.GuildId == guildId && w.WarningCount <= warningCount)
                                .OrderByDescending(x => x.WarningCount)
                                .FirstOrDefaultAsync();
            string reason = $"Automatic escalation: {warningCount} warnings";
            
            if (escalationRule is null)
            {
                return;
            }

            // Build the emebed for the user message and logging channel.
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Automatic escalation. {escalationRule.Punishment}")
                .AddField("User:", $"{user.Mention}")
                .AddField("User ID:", $"```{user.Id}```")
                .AddField("Moderator:", moderator!.Mention)
                .AddField("Reason:", $"```{reason}```")
                .WithThumbnail(user.AvatarUrl)
                .WithFooter($"{moderator.GlobalName}", moderator.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            int? duration = escalationRule.DurationHours;
            if (escalationRule.DurationHours.HasValue)
            {
                embed.AddField("Duration:", $"```{duration} hours. ({duration/24} days)```");
            };

            // Send DM to user regarding the escalation .
            await user.SendMessageAsync(embed);

            // Switch for handling escalation  types.
            switch (escalationRule.Punishment)
            {
                case WarningEscalationType.ban:
                {
                    await guild.BanMemberAsync(userId, reason: reason);
                    if (duration.HasValue)
                    {
                        DateTime date = DateTime.UtcNow.AddHours(duration.Value);
                        await taskSchedulerService.AddTaskAsync(TaskType.UnBan, guildId, userId, date);
                    }
                    await moderationLogService.LogModeratorActionAsync(
                            guildId,
                            userId,
                            client.CurrentApplication.Id,
                            ModerationType.ban.ToString(),
                            reason,
                            image: null,
                            embed: embed);
                    break;
                }

                case WarningEscalationType.kick:
                {
                    await guild.RemoveMemberAsync(userId, reason: reason);
                    await moderationLogService.LogModeratorActionAsync(
                            guildId,
                            userId,
                            client.CurrentApplication.Id,
                            ModerationType.kick.ToString(),
                            reason,
                            image: null,
                            embed: embed);
                    break;
                }

                case WarningEscalationType.Mute:
                {
                    ulong? mutedRoleId = await dbContext.GuildSettings.Where(w => w.GuildId == guildId).Select(s => s.MutedRoleId).FirstOrDefaultAsync(); 
                    if (mutedRoleId is null)
                    {
                        break;
                    }
                    await user.GrantRoleAsync(await guild.GetRoleAsync(mutedRoleId.Value));
                    if (duration.HasValue)
                    {
                        DateTime date = DateTime.UtcNow.AddHours(duration.Value);
                        await taskSchedulerService.AddTaskAsync(TaskType.UnMute, guildId, userId, date);
                    }
                    await moderationLogService.LogModeratorActionAsync(
                            guildId,
                            userId,
                            client.CurrentApplication.Id,
                            ModerationType.mute.ToString(),
                            reason,
                            image: null,
                            embed: embed);
                    break;
                }

                default:
                {
                    break;
                }
            }
        }
        #endregion 
    }
}
