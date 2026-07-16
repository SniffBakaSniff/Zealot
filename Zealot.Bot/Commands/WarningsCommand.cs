using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    [Command("warnings")]
    public class WarningsCommand(IWarningService warningService)
    {
        // TODO: Add a way to view a specific warning's details for an un truncated reason (e.g., /warninginfo warningId) and include the full reason, date, and moderator information in the response.
        [Command("view")]
        [Description("Fetches warnings for a user from the database. (Testing purposes)")]
        [PermissionCheck(CommandPermissions.ViewWarnings, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task FetchWarningsAsync(
            SlashCommandContext ctx,
            [Description("The user to fetch warnings for.")] DiscordMember? user = null)
        {
            if (ctx.Guild is null)
            {
                await ctx.RespondAsync("This command can only be used in a server.");
                return;
            }

            IEnumerable<WarningData> warnings;

            if (user is not null)
            {
                warnings = await warningService.GetWarningsForUserAsync(
                    ctx.Guild.Id,
                    user.Id);
            }
            else
            {
                warnings = await warningService.GetPaginatedWarningsForGuildAsync(
                    ctx.Guild.Id);
            }

            if (!warnings.Any())
            {
                var noWarningsEmbed = new DiscordEmbedBuilder()
                    .WithDescription(user is not null ? $"{user.Mention} has no warnings." : "No warnings found.");
                await ctx.RespondAsync(noWarningsEmbed);
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gray);

            if (user is not null)
            {
                embed.WithTitle($"Warnings for {user.Username}").WithThumbnail(user.GetAvatarUrl(DSharpPlus.MediaFormat.Auto));
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
                    .AddEmbed(embed));
        }
    }
}
