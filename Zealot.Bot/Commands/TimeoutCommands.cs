using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    [Command("timeout")]
    public class SlashTimeoutCommands(ITimeoutService timeoutService)
    {

        [Command("user")]
        [Description("Timeout a user.")]
        [PermissionCheck(CommandPermissions.TimeoutMembers, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task TimeoutCommand(SlashCommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to timeout.")] DiscordMember target,
            [Description("Duration of the mute.")] TimeoutDuration duration,
            [Description("The reason for the timeout.")] string? reason = null,
            [Description("An image to be attached as reference or evidence for this log entry")] DiscordAttachment? image = null)
        {

            if (target.IsOwner)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because they are the Owner.")
                        .AsEphemeral(true));

                return;
            }

            if (target.Permissions.HasPermission(DiscordPermission.Administrator))
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because they are an Administrator.")
                        .AsEphemeral(true));

                return;
            }

            // Check if the moderator is higher in the hierarchy than the target.
            if (target.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));

                return;
            }
            
            var durationSpan = duration == TimeoutDuration.ThirtyMinutes
                ? TimeSpan.FromMinutes(30)
                : TimeSpan.FromHours((int)duration);

            var embed = await timeoutService.TimeoutUserAsync(
                ctx.Guild!,
                ctx.Member!,
                target,
                durationSpan,
                reason,
                image);

            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed!));
        }

        [Command("remove")]
        [Description("Removes a Timeout for a user.")]
        [PermissionCheck(CommandPermissions.RemoveTimeout, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task RemoveTimeoutAsync(SlashCommandContext ctx,
            [RequireHigherUserHierarchy][Description("The user to unmute.")] DiscordMember target,
            [Description("The reason for the unmute.")] string? reason = null)
        {
            // Check if the moderator is higher in the hierarchy than the target.
            if (target.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot remove the timeout for this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));
                return;
            }

            if (!target.IsTimedOut)
            {
                var notMutedEmbed = new DiscordEmbedBuilder()
                    .WithDescription("This user is not currently on Timeout.")
                    .WithColor(DiscordColor.Gray);
                var notMutedResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(notMutedEmbed)
                    .AsEphemeral(true);
                await ctx.RespondAsync(notMutedResponse);
                return;
            }

            var embed = await timeoutService.RemoveTimeoutAsync(
                ctx.Guild!,
                ctx.Member!,
                target,
                reason);

            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed));
    
        }
    }

    public class TextTimeoutCommands(ITimeoutService timeoutService)
    {
        [Command("to")]
        [PermissionCheck(CommandPermissions.TimeoutMembers, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task PrefixTimeoutAsync(
            TextCommandContext ctx,
            [RequireHigherUserHierarchy] DiscordMember target,
            int amount,
            string unit,
            [RemainingText] string? reason = null)
        {
            if (target is null)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Invalid target! please @mention the person you want to timeout.")
                        .AsEphemeral(true));
                return;
            }

            if (target.IsOwner)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because they are the Owner.")
                        .AsEphemeral(true));
                return;
            }

            if (target.Permissions.HasPermission(DiscordPermission.Administrator))
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because they are an Administrator.")
                        .AsEphemeral(true));
                return;
            }

            // Check if the moderator is higher in the hierarchy than the target.
            if (target.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));
                return;
            }

            TimeSpan duration = unit.ToLowerInvariant() switch
            {
                "m" or "min" or "mins" or "minute" or "minutes"
                    => TimeSpan.FromMinutes(amount),

                "h" or "hr" or "hrs" or "hour" or "hours"
                    => TimeSpan.FromHours(amount),

                "d" or "day" or "days"
                    => TimeSpan.FromDays(amount),

                "w" or "week" or "weeks"
                    => TimeSpan.FromDays(amount * 7),

                _ => TimeSpan.Zero
            };

            if (duration == TimeSpan.Zero)
            {
                await ctx.RespondAsync(
                    "Invalid duration. Examples: `30 minutes`, `12 hours`, `3 days`, `1 week`.");
                return;
            }

            if (duration > TimeSpan.FromDays(28))
            {
                await ctx.RespondAsync("Discord only allows timeouts up to 28 days.");
                return;
            }

            var embed = await timeoutService.TimeoutUserAsync(
                ctx.Guild!,
                ctx.Member!,
                target,
                duration,
                reason,
                image: null);

            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed));
        }


        [Command("unto")]
        [PermissionCheck(CommandPermissions.RemoveTimeout, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task PrefixRemoveTimeoutAsync(
            TextCommandContext ctx,
            [RequireHigherUserHierarchy] DiscordMember target,
            [RemainingText] string? reason = null)
        {
            if (target.IsOwner)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot remove the timeout for this user because they are the Owner.")
                        .AsEphemeral(true));
                return;
            }

            // Check if the moderator is higher in the hierarchy than the target.
            if (target.Hierarchy >= ctx.Guild!.CurrentMember.Hierarchy)
            {
                await ctx.RespondAsync(
                    new DiscordInteractionResponseBuilder()
                        .WithContent("I cannot timeout this user because their highest role is equal to or higher than mine.")
                        .AsEphemeral(true));
                return;
            }

            if (!target.IsTimedOut)
            {
                var notMutedEmbed = new DiscordEmbedBuilder()
                    .WithDescription("This user is not currently on Timeout.")
                    .WithColor(DiscordColor.Gray);
                var notMutedResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(notMutedEmbed)
                    .AsEphemeral(true);
                await ctx.RespondAsync(notMutedResponse);
                return;
            }

            var embed = await timeoutService.RemoveTimeoutAsync(
                ctx.Guild!,
                ctx.Member!,
                target,
                reason);

            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed));
        }
    }
}