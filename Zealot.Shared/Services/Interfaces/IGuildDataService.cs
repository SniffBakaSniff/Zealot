using DSharpPlus;
using DSharpPlus.Entities;
using Zealot.Shared.Database.Models;

namespace Zealot.Shared.Services.Interfaces
{
    public interface IGuildDataService
    {
        /// <summary>
        /// Adds the guild information to the database.
        /// </summary>
        /// <param name="guild">The guild for which to add information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddClientGuilds(DiscordGuild guild);

        /// <summary>
        /// Removes the guild information from the database.
        /// </summary>
        /// <param name="guildId">The ID of the guild to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveClientGuild(ulong guildId);

        /// <summary>
        /// Gets all guilds from the database.
        /// </summary>
        /// <returns>A list of all guilds stored in the database.</returns>
        Task<List<GuildsData>> GetAllGuildsAsync();

        /// <summary>
        /// Gets a specific guild by its ID.
        /// </summary>
        /// <param name="guildId">The ID of the guild to retrieve.</param>
        /// <returns>The guild information associated with the specified ID, or null if not found.</returns>
        Task<GuildsData?> GetGuildByIdAsync(ulong guildId);

        /// <summary>
        /// Gets guilds by a list of IDs. Compares the Ids with the database and returns the guilds that match.
        /// </summary>
        /// <param name="guildIds">The IDs of the guilds to retrieve.</param>
        /// <returns>A list of guilds associated with the specified IDs.</returns>
        Task<List<GuildsData>> GetGuildsByIdsAsync(IEnumerable<ulong> guildIds);
    }
}