using DSharpPlus.Entities;

using Zealot.Database.Models;

namespace Zealot.Services.Interfaces
{
    public interface IModerationLogService
    {
        /// <summary>
        /// Logs a moderator action (e.g., ban, warn, mute) to the database for future reference.
        /// </summary>
        /// <param name="guildId">The ID of the guild where the action took place.</param>
        /// <param name="userId">The ID of the user who was moderated.</param>
        /// <param name="moderatorId">The ID of the moderator who performed the action.</param>
        /// <param name="actionType">The type of moderation action (e.g., "ban", "warn").</param>
        /// <param name="reason">The reason for the action, if provided.</param>
        /// <param name="duration">The duration of the action (for temporary actions), or null if permanent.</param>
        /// <param name="contextMessage">Additional context such as a message link or description.</param>
        /// <param name="expiresAt">When the action should expire, if applicable. (e.g., Temporary Mutes)</param>
        /// <param name="embed">The DiscordEmbed to be sent to the logging channel.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task LogModeratorActionAsync(
            ulong guildId,
            ulong? userId,
            ulong moderatorId,
            string actionType,
            string? reason = null,
            TimeSpan? duration = null,
            string? contextMessage = null,
            DateTimeOffset? expiresAt = null,
            DiscordEmbed? embed = null,
            DiscordAttachment? image = null);

        /// <summary>
        /// Retrieves a filtered list of moderator logs based on the specified parameters. 
        /// Supports combining multiple filters such as guild, user, moderator, action type, 
        /// case number, and date ranges.
        /// </summary>
        /// <param name="guildId">Optional ID of the guild to filter logs for.</param>
        /// <param name="userId">Optional ID of the user who was moderated.</param>
        /// <param name="moderatorId">Optional ID of the moderator who performed the action.</param>
        /// <param name="actionType">Optional type of moderation action (e.g., "ban", "warn").</param>
        /// <param name="caseNumber">Optional case number for directly locating a specific log entry.</param>
        /// <param name="createdAfter">Optional lower bound for the creation timestamp.</param>
        /// <param name="createdBefore">Optional upper bound for the creation timestamp.</param>
        /// <returns>A list of moderator log entries that match the provided filters.</returns>
        Task<IEnumerable<ModeratorLogsDTO>> GetModeratorLogsAsync(
        ulong? guildId = null,
        ulong? userId = null,
        ulong? moderatorId = null,
        string? actionType = null,
        int? caseNumber = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? createdBefore = null,
        int page = 1,
        int pageSize = 10);

        /// <summary>
        /// Retrieves a single moderator log entry by case number for the specified guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild the log belongs to.</param>
        /// <param name="caseNumber">The case number of the log to retrieve.</param>
        /// <returns>The matching <c>ModeratorLogs</c> entry, or <c>null</c> if not found.</returns>
        Task<ModeratorLogs?> GetModerationLogByCaseNumberAsync(ulong guildId, int caseNumber);

        /// <summary>
        /// Validates whether the provided attachment is an image and meets size constraints.
        /// </summary>
        /// <param name="attachment">The Discord attachment to validate.</param>
        /// <returns>
        /// A <see cref="DiscordInteractionResponseBuilder"/> containing an error message if the attachment is invalid;
        /// otherwise, <c>null</c> if the attachment is valid.
        /// </returns>
        Task<DiscordInteractionResponseBuilder?> IsValidAttachment(DiscordAttachment attachment);
    }
}