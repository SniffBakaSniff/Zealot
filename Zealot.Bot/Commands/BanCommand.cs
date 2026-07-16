using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Serilog;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    public class BanCommands(IModerationLogService moderationLogService)
    {
        [Command("ban")]
        [Description("Bans a user from the server with an optional message deletion timeframe.")]
        [PermissionCheck(CommandPermissions.BanMembers, defaultPermission: DiscordPermission.BanMembers)]
        public async Task BanCommandAsync(SlashCommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to ban from the server.")] DiscordMember target,
            [Description("The reason for the ban.")] string reason,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null,
            [Description("How much of the user's recent message history to delete.")] TimeFrame deleteMessages = TimeFrame.None)
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

            if (target is null)
            {
                var noUserEmbed = new DiscordEmbedBuilder().WithDescription("No user selected.").WithColor(DiscordColor.Gray);
                await ctx.RespondAsync(noUserEmbed);
            }

            // Defer the repsonse
            // No real reason for this but imma leave it for now
            await ctx.DeferResponseAsync();

            // Get the member from the guild
            var member = await ctx.Guild!.GetMemberAsync(target!.Id);

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
                    .WithTitle($"You have been banned from {ctx.Guild.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (reason is not null)
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
            .AddEmbed(embed);

            // Log the ban 
            await moderationLogService.LogModeratorActionAsync(
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

        [Command("unban")]
        [Description("Unbans a user from the server.")]
        [PermissionCheck(CommandPermissions.UnbanMembers, defaultPermission: DiscordPermission.BanMembers)]
        public async Task UnBanCommandAsync(SlashCommandContext ctx,
            [Description("The UserID of the individual you want to unban.")] ulong userId,
            [Description("The reason for the unban.")] string? reason = null)
        {
            // Fetch the user
            var user = await ctx.Client.GetUserAsync(userId, true);

            // Check if the user is banned (Returns a NotFound if user isnt banned?? like why?? no easy null check for me 😭)
            try
            {
                var banned = await ctx.Guild!.GetBanAsync(user);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                // User is not banned, send ephemeral message
                var noBanEmbed = new DiscordEmbedBuilder()
                    .WithDescription($"The user {user.Mention} is not banned.")
                    .WithColor(DiscordColor.Gray);

                var noBanResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(noBanEmbed);

                await ctx.RespondAsync(noBanResponse);
                return;
            }

            // Create a invite url
            var channel = ctx.Guild!.GetDefaultChannel();
            var invite = await channel!.CreateInviteAsync(max_age: 0, max_uses: 0, temporary: false, unique: true);
            string inviteUrl = invite.ToString();

            // Create an embed to be sent to the unbanned user
            var dmEmbed = new DiscordEmbedBuilder()
                .WithDescription($"You have been unbanned from [{ctx.Guild!.Name}]({inviteUrl})")
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTime.UtcNow);

            // Try and DM the user
            try
            {
                await user.SendMessageAsync(dmEmbed);
            }
            catch (Exception ex) { Log.Error(ex, "Failed to send unban DM."); } // Do nothing if the DM fails

            // Build an embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("User Unbanned.")
                .AddField("User:", $"{user.Mention}")
                .AddField("User ID:", $"```{user.Id}```")
                .AddField("Moderator:", ctx.User.Mention)
                .WithThumbnail(user.AvatarUrl)
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            // Only add the reason field if a reason is given.
            if (reason is not null)
            {
                embed.AddField("Reason:", $"```{reason}```");
            }

            // Unban the user from the guild
            await ctx.Guild!.UnbanMemberAsync(userId, reason);

            // Build response
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed);

            // Respond to the command user
            await ctx.RespondAsync(response);

            // Log the unban
            await moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                user.Id,
                ctx.User.Id,
                ModerationType.unban.ToString(),
                reason,
                embed: embed);
        }
    }
}