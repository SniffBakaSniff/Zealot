using DSharpPlus.Entities;
using Zealot.Shared.Enums;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Shared.Services
{
    public class TimeoutService(IModerationLogService moderationLogService)
        : ITimeoutService
    {
        public async Task<DiscordEmbedBuilder> TimeoutUserAsync(
            DiscordGuild guild,
            DiscordMember moderator,
            DiscordMember target,
            TimeSpan duration,
            string? reason,
            DiscordAttachment? image = null)
        {
            if (target.IsOwner)
                throw new InvalidOperationException("I cannot timeout the server owner.");

            if (target.Permissions.HasPermission(DiscordPermission.Administrator))
                throw new InvalidOperationException("I cannot timeout an administrator.");

            if (target.Hierarchy >= guild.CurrentMember.Hierarchy)
                throw new InvalidOperationException("I cannot timeout this user because their highest role is equal to or higher than mine.");

            if (image is not null)
            {
                var error = await moderationLogService.IsValidAttachment(image);
                if (error is not null)
                    return new DiscordEmbedBuilder().WithDescription("Invalid image attachment.").WithColor(DiscordColor.Gray);
            }

            // Attempt to DM the user
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"You have been timed out in {guild.Name}")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(reason))
                    dmEmbed.AddField("Reason", $"```{reason}```");

                dmEmbed.AddField("Duration", duration.ToString(@"d\.hh\:mm"));

                await target.SendMessageAsync(dmEmbed);
            }
            catch
            {
                // Ignore DM failures
            }

            var expiresAt = DateTimeOffset.UtcNow.Add(duration);
            var unixTimestamp = expiresAt.ToUnixTimeSeconds();

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{target.GlobalName ?? target.Username} has been timed out.")
                .AddField("User", target.Mention, true)
                .AddField("User ID", $"```{target.Id}```", true)
                .AddField("Moderator", moderator.Mention)
                .AddField("Until", $"<t:{unixTimestamp}:f> (<t:{unixTimestamp}:R>)")
                .WithThumbnail(target.AvatarUrl)
                .WithFooter(moderator.GlobalName ?? moderator.Username, moderator.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            if (!string.IsNullOrWhiteSpace(reason))
                embed.AddField("Reason", $"```{reason}```");

            if (image is not null)
                embed.WithImageUrl(image.Url!);

            await target.TimeoutAsync(expiresAt);

            await moderationLogService.LogModeratorActionAsync(
                guild.Id,
                target.Id,
                moderator.Id,
                ModerationType.timeout.ToString(),
                reason,
                duration,
                image: image,
                embed: embed);

            return embed;
        }

        public async Task<DiscordEmbedBuilder> RemoveTimeoutAsync(
            DiscordGuild guild,
            DiscordMember moderator,
            DiscordMember target,
            string? reason = null)
        {
            if (target.Hierarchy >= guild.CurrentMember.Hierarchy)
                throw new InvalidOperationException(
                    "I cannot remove the timeout for this user because their highest role is equal to or higher than mine.");

            if (!target.IsTimedOut)
                throw new InvalidOperationException("This user is not currently timed out.");

            // Attempt to DM the user
            try
            {
                var dmEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"Your timeout in {guild.Name} has been removed.")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(reason))
                    dmEmbed.AddField("Reason", $"```{reason}```");

                await target.SendMessageAsync(dmEmbed);
            }
            catch
            {
                // Ignore DM failures
            }
            
            // Remove the timeout
            await target.TimeoutAsync(null);

            // Build the log/response embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{target.GlobalName ?? target.Username} is no longer timed out.")
                .AddField("User", target.Mention, true)
                .AddField("User ID", $"```{target.Id}```", true)
                .AddField("Moderator", moderator.Mention)
                .WithThumbnail(target.AvatarUrl)
                .WithFooter(
                    moderator.GlobalName ?? moderator.Username,
                    moderator.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gray);

            if (!string.IsNullOrWhiteSpace(reason))
                embed.AddField("Reason", $"```{reason}```");

            await moderationLogService.LogModeratorActionAsync(
                guild.Id,
                target.Id,
                moderator.Id,
                ModerationType.RemoveTimeout.ToString(),
                reason,
                embed: embed);

            return embed;
        }
    }
}