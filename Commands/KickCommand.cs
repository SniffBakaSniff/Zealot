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
            [Description("An image contatining evedince")] DiscordAttachment? image = null,
            [Description("Whether to send the kick reason to the user via DM.")] bool sendReason = true,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            // Check if the target is an admin, bot, or the person issuing the command
            if (target.Permissions.HasPermission(DiscordPermission.Administrator) ||
                target.Id == ctx.User.Id ||
                target.IsBot)
            {
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithDescription("You cannot kick this user. They are an administrator, a bot, or yourself.")
                    .WithColor(DiscordColor.Gray);

                await ctx.RespondAsync(embed: errorEmbed);
                return;
            }

            // Make sure the image is withen certain parameters
            if (image is not null && !image!.MediaType!.StartsWith("image/") ||
                image is not null && image!.FileSize > 512_000) // 512kB
            {
                // Create and send an ephemeral embed stating that the image is not within the parameters
                var badImageEmbed = new DiscordEmbedBuilder()
                    .WithDescription("The attachement you provided is either not a valid image is greater then 512kB.")
                    .WithColor(DiscordColor.Gray);

                var badImageResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(badImageEmbed)
                    .AsEphemeral(true);

                await ctx.RespondAsync(badImageResponse);
                return;
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
            await ctx.RespondAsync(response);

            await ctx.Guild!.RemoveMemberAsync(target, reason);
        }
    }
}