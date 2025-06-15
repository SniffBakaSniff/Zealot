using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("ban")]
        [Description("Bans a user from the server with an optional message deletion timeframe.")]
        [RequirePermissions(DiscordPermission.BanMembers)]
        public async Task BanCommand(CommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to ban from the server.")] DiscordMember target,
            [Description("The reason for the ban.")] string reason,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null,
            [Description("How much of the user's recent message history to delete.")] TimeFrame deleteMessages = TimeFrame.None,
            [Description("Whether to send the ban reason to the user via DM.")] bool sendReason = true,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            // Defer the repsonse
            await ctx.DeferResponseAsync();

            // Get the member from the guild
            var member = await ctx.Guild!.GetMemberAsync(target.Id);

            // Convert the TimeFrame to a TimeSpan for discord
            TimeSpan deleteSpan = TimeSpan.FromHours((int)deleteMessages);

            // Check if the target is an admin, bot, or the person issuing the command
            if (member.Permissions.HasPermission(DiscordPermission.Administrator) ||
                target.Id == ctx.User.Id ||
                target.IsBot)
            {
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithDescription("You cannot ban this user. They are an administrator, a bot, or yourself.")
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
                    .WithTitle($"You have been banned from {ctx.Guild.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (sendReason && reason is not null)
                    dmEmbed.AddField("Reason", $"```{reason}```");

                await target.SendMessageAsync(dmEmbed);
            }
            catch { } // Do nothing if the DM fails

            // Create the ban messages embed (used for logs channel as well)
            var embed = new DiscordEmbedBuilder()
                .WithTitle("User banned.")
                .AddField("User:", $"{target.Mention}")
                .AddField("User ID:", $"```{target.Id}```")
                .AddField("Moderator:", ctx.User.Mention)
                .WithThumbnail(target.AvatarUrl)
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            // Only add the reason field if a reason is given.
            if (reason is not null)
            {
                embed.AddField("Reason:", $"```{reason}```");
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
                ModerationType.ban.ToString(),
                reason,
                image: image,
                embed: embed);

            // Respond the the user
            await ctx.EditResponseAsync(response);

            // Ban the user
            //await ctx.Guild.BanMemberAsync(target.Id, deleteSpan, $"{reason} (Banned by {ctx.User.Username})");
        }
    }
}