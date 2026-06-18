using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Zealot.Shared.Database.Models;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        // TODO: Add a way to view a specific warning's details for an un truncated reason (e.g., /warninginfo warningId) and include the full reason, date, and moderator information in the response.
        [Command("warnings")]
        [Description("Fetches warnings for a user from the database. (Testing purposes)")]
        [RequirePermissions(DiscordPermission.ModerateMembers)]
        public async Task FetchWarningsAsync(
            CommandContext ctx,
            [Description("The user to fetch warnings for.")] DiscordMember? user = null,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            if (ctx.Guild is null)
            {
                await ctx.RespondAsync("This command can only be used in a server.");
                return;
            }

            IEnumerable<WarningData> warnings;

            if (user is not null)
            {
                warnings = await _warningService.GetWarningsForUserAsync(
                    ctx.Guild.Id,
                    user.Id);
            }
            else
            {
                warnings = await _warningService.GetPaginatedWarningsForGuildAsync(
                    ctx.Guild.Id);
            }

            if (!warnings.Any())
            {
                await ctx.RespondAsync(
                    user is not null
                        ? $"{user.Mention} has no warnings."
                        : "No warnings found.");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gray);

            if (user is not null)
            {
                embed
                    .WithTitle($"Warnings for {user.Username}")
                    .WithThumbnail(user.GetAvatarUrl(DSharpPlus.MediaFormat.Auto));
            }
            else
            {
                embed.WithTitle("Recent Guild Warnings");
            }

            foreach (var warning in warnings.Take(10))
            {
                string moderatorMention;

                try
                {
                    var moderator = await ctx.Client.GetUserAsync(warning.ModeratorId);
                    moderatorMention = moderator.Mention;
                }
                catch
                {
                    moderatorMention = $"Unknown Moderator ({warning.ModeratorId})";
                }

                if (warning.Reason != null && warning.Reason.Length > 50)
                {
                    warning.Reason = warning.Reason[..50] + "...";
                }

                var userMention = await ctx.Client.GetUserAsync(warning.UserId);

                embed.AddField(
                    $"Warning ID: {warning.WarningId}",
                    $"User: {userMention.Mention}\n" +
                    $"Reason: {warning.Reason ?? "No reason provided"}\n" +
                    $"Moderator: {moderatorMention}\n" +
                    $"Date: <t:{new DateTimeOffset(warning.CreatedAt).ToUnixTimeSeconds()}:F>");
            }

            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral(ephemeral));
        }
    }
}
