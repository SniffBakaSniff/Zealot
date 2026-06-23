using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;
using Zealot.Shared.Enums;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        [Command("unmute")]
        [Description("Unmutes a user by removing the muted role.")]
        [RequirePermissions(DiscordPermission.ModerateMembers)]
        public async Task UnmuteCommand(CommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to unmute.")] DiscordMember target,
            [Description("The reason for the unmute.")] string? reason = null,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
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

            ulong? mutedRoleId = await _guildSettingService.GetMutedRoleIdAsync(ctx.Guild!.Id);

            if (mutedRoleId is null)
            {
                var noRoleEmbed = new DiscordEmbedBuilder()
                    .WithDescription("There is no muted role set up in this server.")
                    .WithColor(DiscordColor.Gray);

                var noRoleResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(noRoleEmbed).AsEphemeral(true);

                await ctx.RespondAsync(noRoleResponse);
                return;
            }

            var muteRole = await ctx.Guild.GetRoleAsync(mutedRoleId.Value);

            if (!target.Roles.Contains(muteRole))
            {
                var notMutedEmbed = new DiscordEmbedBuilder()
                    .WithDescription("This user is not currently muted.")
                    .WithColor(DiscordColor.Gray);

                var notMutedResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(notMutedEmbed)
                    .AsEphemeral(true);

                await ctx.RespondAsync(notMutedResponse);
                return;
            }

            // Attempt to DM the target
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"You have been unmuted in {ctx.Guild.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                await target.SendMessageAsync(dmEmbed);
            }
            catch { } // DM failure is ignored

            // Build the response embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("User has been unmuted.")
                .AddField("User:", $"{target.Mention}")
                .AddField("User ID:", $"```{target.Id}```")
                .AddField("Moderator:", ctx.User.Mention)
                .WithThumbnail(target.AvatarUrl)
                .WithFooter($"{ctx.User.GlobalName}", ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            if (reason is not null)
            {
                embed.AddField("Reason:", $"```{reason}```");
            }

            // Remove the muted role
            await target.RevokeRoleAsync(muteRole);

            // Cancel scheduled unmute if it exists
            await _taskSchedulerService.RemoveTaskAsync(TaskType.UnMute, ctx.Guild.Id, target.Id);

            // Log the unmute
            await _moderationLogService.LogModeratorActionAsync(
                ctx.Guild.Id,
                target.Id,
                ctx.User.Id,
                ModerationType.unmute.ToString(),
                reason,
                embed: embed);

            // Build and send the response
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(ephemeral);

            await ctx.RespondAsync(response);
        }
    }
}
