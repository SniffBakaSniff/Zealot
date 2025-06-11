using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Serilog;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("unban")]
        [Description("Unbans a user from the server.")]
        [RequirePermissions(DiscordPermission.BanMembers)]
        public async Task UnBanCommand(CommandContext ctx,
            [Description("The UserID of the individual you want to unban.")] ulong userId,
            [Description("The reason for the unban.")] string? reason = null,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
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
                    .AddEmbed(noBanEmbed)
                    .AsEphemeral(ephemeral);

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

            // Log the unban
            await _moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                user.Id,
                ctx.User.Id,
                ModerationType.unban.ToString(),
                reason,
                embed: embed);

            // Build response
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(ephemeral);

            // Respond to the command user
            await ctx.RespondAsync(response);

            // Unban the user from the guild
            await ctx.Guild!.UnbanMemberAsync(userId, reason);
        }
    }
}
