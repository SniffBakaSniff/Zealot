using DSharpPlus.Entities;

namespace Zealot.Shared.Services.Interfaces
{
    public interface ITimeoutService
    {
        Task<DiscordEmbedBuilder> TimeoutUserAsync(
            DiscordGuild guild,
            DiscordMember moderator,
            DiscordMember target,
            TimeSpan duration,
            string? reason,
            DiscordAttachment? image = null);

        Task<DiscordEmbedBuilder> RemoveTimeoutAsync(
            DiscordGuild guild,
            DiscordMember moderator,
            DiscordMember target,
            string? reason = null);
    }
}