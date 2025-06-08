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
    }
}