using Zealot.Shared.Database.Models;

namespace Zealot.Shared.Services.Interfaces
{
    public interface IWarningService
    {
        /// <summary>
        /// Adds a warning for a user in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild where the warning is being added.</param>
        /// <param name="userId">The ID of the user receiving the warning.</param>
        /// <param name="moderatorId">The ID of the moderator adding the warning.</param>
        /// <param name="reason">The reason for the warning (optional).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddWarningAsync(ulong guildId, ulong userId, ulong moderatorId, string? reason = null);

        /// <summary>
        /// Retrieves all warnings for a specific guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve warnings.</param>
        /// <returns>A list of warnings associated with the specified guild.</returns>
        Task<IEnumerable<WarningData>> GetWarningsForGuildAsync(ulong guildId);

        /// <summary>
        /// Retrieves a paginated list of warnings for a specific guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve warnings.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of warnings per page.</param>
        /// <returns>A list of warnings associated with the specified guild.</returns>
        Task<IEnumerable<WarningData>> GetPaginatedWarningsForGuildAsync(ulong guildId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Retrieves all warnings for a specific user in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve warnings.</param>
        /// <param name="userId">The ID of the user for whom to retrieve warnings.</param>
        /// <returns>A list of warnings associated with the specified user and guild.</returns>
        Task<IEnumerable<WarningData>> GetWarningsForUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Retrieves a paginated list of warnings for a specific user in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve warnings.</param>
        /// <param name="userId">The ID of the user for whom to retrieve warnings.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of warnings per page.</param>
        /// <returns>A list of warnings associated with the specified user and guild.</returns>
        Task<IEnumerable<WarningData>> GetPaginatedWarningsForUserAsync(ulong guildId, ulong userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Retrieves the warning associated with a specific warning ID.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve the warning.</param>
        /// <param name="warningId">The ID of the warning to retrieve.</param>
        /// <returns>The warning associated with the specified ID, or null if not found.</returns>
        Task<WarningData?> GetWarningByIdAsync(ulong guildId, int warningId);

        /// <summary>
        /// Gets the total count of warnings for a specific user in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to retrieve the warning count.</param>
        /// <param name="userId">The ID of the user for whom to retrieve the warning count.</param>
        /// <returns>The total count of warnings associated with the specified user and guild.</returns>
        Task<int> GetWarningCountAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Clears all warnings for a specific user in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to clear warnings.</param>
        /// <param name="userId">The ID of the user for whom to clear warnings.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearWarningsForUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Clears a specific warning by its ID.
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to clear the warning.</param>
        /// <param name="warningId">The ID of the warning to clear.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearWarningByIdAsync(ulong guildId, int warningId);

        /// <summary>
        /// Handles warning escalation .
        /// </summary>
        /// <param name="guildId">The ID of the guild for which to handle warning escalation .</param>
        /// <param name="userId">The ID of the user for whom to handle warning escalation .</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task WarningescalationAsync(ulong guildId, ulong userId);
    }
}
