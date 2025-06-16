using DSharpPlus.Commands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("logs")]
        [Description("Fetches moderator logs with optional filters.")]
        public async Task LogsCommand(
            CommandContext ctx,
            [Description("Filter by user ID.")] ulong? userId = null,
            [Description("Filter by moderator ID.")] ulong? moderatorId = null,
            [Description("Filter by action type (e.g., warn, ban, mute).")] ModerationType? actionType = null,
            [Description("Only show logs created after this date (YYYY-MM-DDTHH:MM).")] string? createdAfter = null,
            [Description("Only show logs created before this date (YYYY-MM-DDTHH:MM).")] string? createdBefore = null,
            [Description("Page Number.")] int page = 1,
            [Description("Number of items per page.")] int pageSize = 5,
            [Description("Send the response as ephemeral?")] bool ephemeral = true
        )
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
            var logs = await _moderationLogService.GetModeratorLogsAsync(
                guildId: ctx.Guild.Id,
                userId: userId,
                moderatorId: moderatorId,
                actionType: actionType?.ToString(),
                createdAfter: after,
                createdBefore: before,
                pageSize: pageSize,
                page: page
            );

            // Send a message if no logs match the filter.
            if (!logs.Any())
            {
                await ctx.RespondAsync("No logs matched the provided filters.");
                return;
            }

            // Make the embed for the Logs
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"📝 Moderator Logs ({logs.Count()} total)")
                .WithColor(DiscordColor.Gray)
                .WithFooter($"Requested by {ctx.User.Username} | Page {page}", ctx.User.AvatarUrl)
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

            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(ephemeral));
        }
    }
}
