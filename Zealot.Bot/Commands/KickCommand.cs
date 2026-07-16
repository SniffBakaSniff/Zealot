using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    public class KickCommand(IModerationLogService moderationLogService)
    {
        [Command("kick")]
        [Description("Kicks a user from the server.")]
        [PermissionCheck(CommandPermissions.KickMembers, defaultPermission: DiscordPermission.KickMembers)]
        public async Task ExecuteKickCommand(SlashCommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to kick from the server.")] DiscordMember target,
            [Description("The reason for the kick.")] string? reason = null,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null)
        {
            // Check if the moderator is higher in the hierarchy than the target.
            if (target.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot mute this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));

                return;
            }

            // Defer the repsonse
            await ctx.DeferResponseAsync();

            // Check if the target is an admin, bot, or the person issuing the command
            if (target.Permissions.HasPermission(DiscordPermission.Administrator) ||
                target.Id == ctx.User.Id ||
                target.IsBot)
            {
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithDescription("You cannot kick this user. They are an administrator, a bot, or yourself.")
                    .WithColor(DiscordColor.Gray);

                await ctx.EditResponseAsync(embed: errorEmbed);
                return;
            }

            if (image is not null)
            {
                var errorResponse = await moderationLogService.IsValidAttachment(image);
                if (errorResponse is not null)
                {
                    await ctx.EditResponseAsync(errorResponse);
                    return;
                }
            }

            // Attempt to DM the target user
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"You have been kicked from {ctx.Guild!.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (reason is not null)
                    dmEmbed.AddField("Reason", $"```{reason}```");

                await target.SendMessageAsync(dmEmbed);
            }
            catch { } // Do nothing if the DM fails

            // Build and embed for the response
            var embed = new DiscordEmbedBuilder()
                .WithTitle("User Kicked.")
                .AddField("User:", $"{target.Mention}", true)
                .AddField("User ID:", $"```{target.Id}```", false)
                .WithThumbnail(target.AvatarUrl)
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            // Only add the reason field if a reason is given.
            if (reason is not null)
            {
                embed.AddField("Reason:", $"```{reason}```", false);
            }

            if (image is not null)
            {
                embed.WithImageUrl(image.Url!);
            }

            // Kick the target
            await ctx.Guild!.RemoveMemberAsync(target, reason);

            // Build the response
            var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed);

            // Send the response
            await ctx.EditResponseAsync(response);

            // Log the ban
            await moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                target.Id,
                ctx.User.Id,
                ModerationType.kick.ToString(),
                reason,
                image: image,
                embed: embed);
        }
    }
}