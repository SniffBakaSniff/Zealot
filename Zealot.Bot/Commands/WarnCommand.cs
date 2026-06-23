using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Zealot.Shared.Enums;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        [Command("warn")]
        [Description("Issues a warning to a user.")]
        [RequirePermissions(DiscordPermission.ModerateMembers)]
        public async Task Warn(CommandContext ctx,
            [Description("The user to warn.")] DiscordMember user,
            [Description("The reason for the warning.")] string? reason = null,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            // Check if the moderator is higher in the hierarchy than the target.
            if (user.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot warn this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));

                return;
            }

            // Check if the command is used in a guild
            if(ctx.Guild is null)
            {
                await ctx.RespondAsync("This command can only be used in a server.");
                return;
            }

            // Check if the reason is too long
            if(reason != null && reason.Length > 2000)
            {
                await ctx.RespondAsync("The reason cannot be longer than 2000 characters.");
                return;
            }

            // Add the warning to the database
            await _warningService.AddWarningAsync(ctx.Guild!.Id, user.Id, ctx.User.Id, reason);

            // Get the total warning count for the user
            int warningCount = await _warningService.GetWarningCountAsync(ctx.Guild.Id, user.Id);

            //TODO: Add automatic punishment based on warning count (e.g., 3 warnings = 1 day mute, 5 warnings = 3 day mute, 7 warnings = ban, etc.)
            //TODO: Add a way to configure the warning thresholds and corresponding punishments in the database
            //TODO: Add a way to reset warning counts after a certain period of time (e.g., 6 months) or allow moderators to manually reset warning counts for users.
            //TODO: Add a way to remove specific warnings from a user (e.g., /removewarning @user warningId).
            //TODO: Add a way to clear all warnings from a user (e.g., /clearwarnings @user).

            var embed = new DiscordEmbedBuilder()
                .WithTitle("User Warned")
                .WithDescription($"{user.Mention} has been warned. Total Warnings: {warningCount}")
                .AddField("Moderator:", ctx.User.Mention)
                .AddField("Reason", reason ?? "No reason provided.")
                .WithThumbnail(user.GetAvatarUrl(DSharpPlus.MediaFormat.Auto))
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(ephemeral);

            await ctx.RespondAsync(response);

            // Attempt to DM the target user
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                .WithTitle("Warning Issued")
                .WithDescription($"You has been warned. Total Warnings: {warningCount}")
                .AddField("Reason", reason ?? "No reason provided.")
                .AddField("Guild", ctx.Guild.Name)
                .WithThumbnail(user.GetAvatarUrl(DSharpPlus.MediaFormat.Auto))
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

                await user.SendMessageAsync(dmEmbed);
            }
            catch { } // Do nothing if the DM fails

            // Log the Warn 
            await _moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                user.Id,
                ctx.User.Id,
                ModerationType.warn.ToString(),
                reason,
                embed: embed);

            // Check for warning escalation
            await _warningService.WarningescalationAsync(ctx.Guild.Id, user.Id);
        }
    }
}