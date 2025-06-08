using Zealot.Database;
using Zealot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Zealot.Database.Models;

namespace Zealot.Services
{
    public class ModerationLogService : IModerationLogService
    {
        private readonly BotDbContext _dbContext;

        public ModerationLogService(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Task responsible for handling the insertion of log items into the database
        public async Task LogModeratorActionAsync(
            ulong guildId,
            ulong? userId,
            ulong moderatorId,
            string actionType,
            string? reason = null,
            TimeSpan? duration = null,
            string? contextMessage = null,
            DateTimeOffset? expiresAt = null)
        {
            // create a log item 
            var log = new ModeratorLogs
            {
                GuildId = guildId,
                UserId = userId,
                ModeratorId = moderatorId,
                ActionType = actionType,
                Reason = reason,
                Duration = duration,
                ContextMessage = contextMessage,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
            };

            // Get the lase CaseNumber 
            var lastCase = await _dbContext.ModeratorLogs
                .Where(l => l.GuildId == log.GuildId)
                .OrderByDescending(l => l.CaseNumber)
                .Select(l => l.CaseNumber)
                .FirstOrDefaultAsync();

            // Increment the CaseNumber by 1
            log.CaseNumber = lastCase + 1;

            // Add the log to the database and save
            _dbContext.ModeratorLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ModeratorLogs>> GetModeratorLogsAsync(
            ulong? guildId = null,
            ulong? userId = null,
            ulong? moderatorId = null,
            string? actionType = null,
            int? caseNumber = null,
            DateTimeOffset? createdAfter = null,
            DateTimeOffset? createdBefore = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _dbContext.ModeratorLogs.AsQueryable();

            // Filter the query
            if (guildId.HasValue)
                query = query.Where(log => log.GuildId == guildId.Value);

            if (userId.HasValue)
                query = query.Where(log => log.UserId == userId.Value);

            if (moderatorId.HasValue)
                query = query.Where(log => log.ModeratorId == moderatorId.Value);

            if (!string.IsNullOrWhiteSpace(actionType))
                query = query.Where(log => log.ActionType == actionType);

            if (caseNumber.HasValue)
                query = query.Where(log => log.CaseNumber == caseNumber.Value);

            if (createdAfter.HasValue)
                query = query.Where(log => log.CreatedAt >= createdAfter.Value.ToUniversalTime());

            if (createdBefore.HasValue)
                query = query.Where(log => log.CreatedAt <= createdBefore.Value.ToUniversalTime());

            // Order and paginate the query
            query = query.OrderByDescending(log => log.CaseNumber);
            int skip = (page - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            // Send the query
            return await query.ToListAsync();
        }
    }
}
