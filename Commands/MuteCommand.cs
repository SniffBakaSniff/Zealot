using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("mute")]
        [Description("Mutes a user.")]
        [RequirePermissions(DiscordPermission.ModerateMembers)]
        public async Task MuteCommand(CommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to ban from the server.")] DiscordMember target,
            [Description("The reason for the ban.")] string reason,
            [Description("Duration of the ban.")] TimeSpan? duration = null,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null,
            [Description("Whether to send the ban reason to the user via DM.")] bool sendReason = true,
            [Description("Send the response as ephemeral?")] bool ephemeral = false)
        {
            ulong? mutedRoleId = await _guildSettingService.GetMutedRoleIdAsync(ctx.Guild!.Id);

            // Check if a MutedRoleId has been set
            if (mutedRoleId is null)
            {
                // Build the repsonse
                var noRoleEmbed = new DiscordEmbedBuilder()
                    .WithDescription("There is no muted role setup somthing somthing")
                    .WithColor(DiscordColor.Gray);

                var noRoleResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(noRoleEmbed).AsEphemeral(true);

                // Send the response and end the interaction
                await ctx.RespondAsync(noRoleResponse);
                return;
            }

            // Get the muted role
            var muteRole = await ctx.Guild!.GetRoleAsync(mutedRoleId.Value);

            // Check to see if the target has the role
            if (target.Roles.Contains(muteRole))
            {
                var mutedEmebd = new DiscordEmbedBuilder()
                    .WithDescription("This user is already muted.")
                    .WithColor(DiscordColor.Gray);

                var mutedResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(mutedEmebd)
                    .AsEphemeral(true);

                // Send the response and end the interaction
                await ctx.RespondAsync(mutedResponse);
                return;
            }

            // Attempt to DM the target user
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"You have been muted in {ctx.Guild.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (sendReason && reason is not null)
                {
                    dmEmbed.AddField("Reason", $"```{reason}```");
                }

                await target.SendMessageAsync(dmEmbed);
            }
            catch { } // Do nothing if the DM fails

            // Build the response embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("User has been muted.")
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

            // Only add the duration field if a duration is given.
            if (duration is not null)
            {
                var expiresAt = DateTimeOffset.UtcNow.Add(duration.Value);
                var unixTimestamp = expiresAt.ToUnixTimeSeconds();

                embed.AddField("Until:", $"<t:{unixTimestamp}:f> (<t:{unixTimestamp}:R>)");

                // Shedule a task to unmute them
                await _taskSchedulerService.AddTaskAsync(
                    TaskType.UnMute,
                    ctx.Guild.Id,
                    target.Id,
                    expiresAt.UtcDateTime);
            }

            // Only add the image field if an image is given.
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
                ModerationType.mute.ToString(),
                reason,
                duration,
                image: image,
                embed: embed);

            // Apply the role to the user
            await target.GrantRoleAsync(muteRole);

            // Respond to the interaction
            await ctx.RespondAsync(response);
        }
    }
}