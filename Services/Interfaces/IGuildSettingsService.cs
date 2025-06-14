namespace Zealot.Services.Interfaces
{
    public interface IGuildSettingService
    {
        /// <summary>
        /// Retrieves the command prefix configured for the specified guild.
        /// Returns a default prefix if no custom prefix is set.
        /// </summary>
        /// <param name="guildId">The unique identifier of the guild.</param>
        /// <returns>The command prefix for the guild.</returns>
        Task<string> GetGuildPrefixAsync(ulong guildId);

        /// <summary>
        /// Sets or updates the command prefix for the specified guild.
        /// Creates a new settings entry if one does not already exist.
        /// </summary>
        /// <param name="guildId">The unique identifier of the guild.</param>
        /// <param name="prefix">The new command prefix to set.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetGuildPrefixAsync(ulong guildId, string prefix);

        /// <summary>
        /// Retrieves the channel where moderation logs should be sent.
        /// </summary>
        /// <param name="guildId">The unique identifier of the guild.</param>
        /// <returns>The unique identifier for the channel</returns>
        Task<ulong?> GetModerationLogChannelAsync(ulong guildId);

        /// <summary>
        /// Sets the moderation logging channel for the specified guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild to update.</param>
        /// <param name="channelId">The ID of the channel to use for moderation logs.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetModerationLogChannelAsync(ulong guildId, ulong channelId);

        /// <summary>
        /// Sets the muted role ID for a specific guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="roleId">The ID of the role to be used as the muted role.</param>
        Task SetMutedRoleIdAsync(ulong guildId, ulong roleId);

        /// <summary>
        /// Retrieves the muted role ID for a specific guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <returns>The ID of the muted role, or null if not set.</returns>
        Task<ulong?> GetMutedRoleIdAsync(ulong guildId);
        
    }
}