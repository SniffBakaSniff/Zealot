using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    public class WarnCommand(IWarningService warningService, IModerationLogService moderationLogService)
    {
        [Command("warn")]
        [Description("Issues a warning to a user.")]
        [PermissionCheck(CommandPermissions.WarnMembers, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task WarnAsync(SlashCommandContext ctx,
            [Description("The user to warn.")] DiscordMember user,
            [Description("The reason for the warning.")] string? reason = null)
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
            await warningService.AddWarningAsync(ctx.Guild!.Id, user.Id, ctx.User.Id, reason);

            // Get the total warning count for the user
            int warningCount = await warningService.GetWarningCountAsync(ctx.Guild.Id, user.Id);

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
                .AddEmbed(embed);

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
            await moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                user.Id,
                ctx.User.Id,
                ModerationType.warn.ToString(),
                reason,
                embed: embed);

            // Check for warning escalation
            await warningService.WarningescalationAsync(ctx.Guild.Id, user.Id);
        }
    }
}