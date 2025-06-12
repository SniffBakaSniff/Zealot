using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("kick")]
        [Description("Kicks a user from the server.")]
        [RequirePermissions(DiscordPermission.KickMembers)]
        public async Task KickCommand(CommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to kick from the server.")] DiscordMember target,
            [Description("The reason for the kick.")] string? reason = null,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null,
            [Description("Whether to send the kick reason to the user via DM.")] bool sendReason = true,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
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
                var errorResponse = await _moderationLogService.IsValidAttachment(image);
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

                if (sendReason && reason is not null)
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

            // Build the response
            var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral(ephemeral);

            // Log the ban
            await _moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                target.Id,
                ctx.User.Id,
                ModerationType.kick.ToString(),
                reason,
                image: image,
                embed: embed);

            // Send the response
            await ctx.EditResponseAsync(response);

            // Kick the target
            await ctx.Guild!.RemoveMemberAsync(target, reason);
        }
    }
}