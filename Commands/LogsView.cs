using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Serilog;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("viewlog")]
        [Description("Command for viewing a specifc log.")]
        public async Task LogsViewCommand(CommandContext ctx,
        [Description("The case number of the log you want to view.")] int caseNumber,
        [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            // Get the log that corresponds with the case number
            var log = await _moderationLogService.GetModerationLogByCaseNumberAsync(ctx.Guild!.Id, caseNumber);

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
                    .AddFile(fileName, stream)
                    .AsEphemeral(ephemeral);

                await ctx.RespondAsync(builder);
                return;
            }
            
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(ephemeral);

            await ctx.RespondAsync(response);
        }
    }
}