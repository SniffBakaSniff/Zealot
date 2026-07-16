using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using System.ComponentModel;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    [Command("logs")]
    public class LogsCommands(IModerationLogService moderationLogService)
    {
        [Command("view")]
        [Description("Fetches moderator logs with optional filters.")]
        [PermissionCheck(CommandPermissions.ViewLogs, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task LogsCommandAsync(
            SlashCommandContext ctx,
            [Description("Filter by user ID.")] ulong? userId = null,
            [Description("Filter by moderator ID.")] ulong? moderatorId = null,
            [Description("Filter by action type (e.g., warn, ban, mute).")] ModerationType? actionType = null,
            [Description("Only show logs created after this date (YYYY-MM-DDTHH:MM).")] string? createdAfter = null,
            [Description("Only show logs created before this date (YYYY-MM-DDTHH:MM).")] string? createdBefore = null,
            [Description("Page Number.")] int page = 1,
            [Description("Number of items per page.")] int pageSize = 5)
        {
            // Send a message if the command is sent in DMs
            if (ctx.Guild is null)
            {
                await ctx.RespondAsync("This command must be used in a server.");
                return;
            }

            DateTimeOffset? after = null;
            DateTimeOffset? before = null;

            // Parse the createdAfter and createdBefore paramters
            if (!string.IsNullOrWhiteSpace(createdAfter))
            {
                if (DateTimeOffset.TryParse(createdAfter, out var parsedAfter))
                    after = parsedAfter;
                else
                {
                    await ctx.RespondAsync("❌ Invalid `createdAfter` date format. Use ISO format (e.g. `YYYY-MM-DDTHH:MM`).");
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(createdBefore))
            {
                if (DateTimeOffset.TryParse(createdBefore, out var parsedBefore))
                    before = parsedBefore;
                else
                {
                    await ctx.RespondAsync("❌ Invalid `createdBefore` date format. Use ISO format (e.g. `YYYY-MM-DDTHH:MM`).");
                    return;
                }
            }

            // Get the logs using the _moderationLogService
            var logs = await moderationLogService.GetModeratorLogsAsync(
                guildId: ctx.Guild.Id,
                userId: userId,
                moderatorId: moderatorId,
                actionType: actionType?.ToString(),
                createdAfter: after,
                createdBefore: before,
                pageSize: pageSize,
                page: page
            );

            int totalLogs = (await moderationLogService.GetModeratorLogsAsync(ctx.Guild.Id)).Count();
            int totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            // Send a message if no logs match the filter.
            if (!logs.Any())
            {
                await ctx.RespondAsync("No logs matched the provided filters.");
                return;
            }

            // Make the embed for the Logs
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"📝 Moderator Logs ({totalLogs} total)")
                .WithColor(DiscordColor.Gray)
                .WithFooter($"Requested by {ctx.User.Username} | Page {page}/{totalPages}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTimeOffset.UtcNow);

            // Populate the embed with log items
            foreach (var log in logs)
            {
                var unixTimestamp = new DateTimeOffset(log.CreatedAt).ToUnixTimeSeconds();

                var fieldValue =
                    $"User: <@{log.UserId}>\n" +
                    $"Moderator: <@{log.ModeratorId}>\n" +
                    $"Time: <t:{unixTimestamp}:f>";

                if (!string.IsNullOrWhiteSpace(log.Reason))
                    fieldValue += $"\nReason: {log.Reason}";

                embed.AddField($"Case #{log.CaseNumber} - `{log.ActionType}`", fieldValue, inline: false);
            }

            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [Command("specific")]
        [Description("Command for viewing a specifc log.")]
        [PermissionCheck(CommandPermissions.ViewLogs, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task SpecificLogCommandAsync(SlashCommandContext ctx,
        [Description("The case number of the log you want to view.")] int caseNumber)
        {
            // Get the log that corresponds with the case number
            var log = await moderationLogService.GetModerationLogByCaseNumberAsync(ctx.Guild!.Id, caseNumber);

            // Make sure the log exists
            if (log is null)
            {
                await ctx.RespondAsync("That log dosnt exist");
                return;
            }

            // Get the discord user for the moderator
            DiscordUser moderator = await ctx.Client.GetUserAsync(log.ModeratorId);

            // Get the dsicord user if the log has a user id
            DiscordUser? user = null;
            if (log.UserId.HasValue)
            {
                user = await ctx.Client.GetUserAsync(log.UserId.Value);
            }

            // Build and embed for the log
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"📝 Case #{log.CaseNumber} - `{log.ActionType}`")
                .WithColor(DiscordColor.Gray);

            if (user is not null)
                embed.AddField("User", user.Mention);
                embed = embed.WithThumbnail(user!.GetAvatarUrl(DSharpPlus.MediaFormat.Auto));


            embed.AddField("Moderator:", moderator.Mention);

            if (log.Duration.HasValue)
                embed.AddField("Duration", FormatDuration(log.Duration.Value));
                
            if (!string.IsNullOrWhiteSpace(log.Reason))
                embed.AddField("Reason", $"```{log.Reason}```");

            embed.WithFooter($"Created").WithTimestamp(log.CreatedAt);

            // Helper for formatting duration nicely
            string FormatDuration(TimeSpan duration)
            {
                if (duration.TotalDays >= 1)
                    return $"{(int)duration.TotalDays}d {duration.Hours}h";
                if (duration.TotalHours >= 1)
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
                if (duration.TotalMinutes >= 1)
                    return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
                return $"{duration.Seconds}s";
            }

            if (log.Image is not null)
            {
                using var stream = new MemoryStream(log.Image);
                string fileName = $"case_{log.CaseNumber}_image.jpg";

                embed.WithImageUrl($"attachment://{fileName}");

                var builder = new DiscordInteractionResponseBuilder().AddEmbed(embed)
                    .AddFile(fileName, stream);

                await ctx.RespondAsync(builder);
                return;
            }
            
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed);

            await ctx.RespondAsync(response);
        }
    }
}
