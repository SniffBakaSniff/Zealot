using Microsoft.EntityFrameworkCore;

using Zealot.Shared.Services.Interfaces;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Database;

namespace Zealot.Shared.Services
{

    // Consider finding shorter Task names.
    public class WarningService(BotDbContext dbContext) : IWarningService
    {
        private readonly BotDbContext _dbContext = dbContext;

        #region AddWarningAsync
        // This method adds a warning to the database for a specific user in a guild. 
        public async Task AddWarningAsync(ulong guildId, ulong userId, ulong moderatorId, string? reason = null)
        {
            if (reason?.Length > 2000)
            {
                throw new ArgumentException("Reason cannot exceed 2000 characters.");
            }

            var warning = new WarningData
            {
                GuildId = guildId,
                UserId = userId,
                ModeratorId = moderatorId,
                Reason = reason
            };

            int nextId = (await _dbContext.WarningData
                .Where(x => x.GuildId == guildId)
                .MaxAsync(x => (int?)x.WarningId) ?? 0) + 1;

            warning.WarningId = nextId;

            await _dbContext.WarningData.AddAsync(warning);
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region GetWarningsForGuildAsync
        // This Method retrieves all warnings for a guild.
        public async Task<IEnumerable<WarningData>> GetWarningsForGuildAsync(ulong guildId)
        {
            return await _dbContext.WarningData.Where(w => w.GuildId == guildId).ToListAsync();
        }
        #endregion

        #region GetPaginatedWarningsForGuildAsync
        // This Method retrieves a paginated list of warnings for a guild.
        public async Task<IEnumerable<WarningData>> GetPaginatedWarningsForGuildAsync(ulong guildId, int page = 1, int pageSize = 20)
        {
            return await _dbContext.WarningData
                .Where(w => w.GuildId == guildId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        #endregion

        #region GetWarningsForUserAsync
        // This method retrieves all warnings for a specific user in a guild.
        public async Task<IEnumerable<WarningData>> GetWarningsForUserAsync(ulong guildId, ulong userId)
        {
            return await _dbContext.WarningData.Where(w => w.GuildId == guildId && w.UserId == userId).ToListAsync();
        }
        #endregion

        #region GetPaginatedWarningsForUserAsync
        // This method retrieves a paginated list of warnings for a specific user in a guild.
        public async Task<IEnumerable<WarningData>> GetPaginatedWarningsForUserAsync(ulong guildId, ulong userId, int page = 1, int pageSize = 20)
        {
            return await _dbContext.WarningData
                .Where(w => w.GuildId == guildId && w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        #endregion

        #region GetWarningsByIdAsync
        // This method retrieves a specific warning by its ID for a user in a guild.
        public async Task<WarningData?> GetWarningByIdAsync(ulong guildId, int warningId)
        {
            return await _dbContext.WarningData.Where(w => w.GuildId == guildId && w.WarningId == warningId).FirstOrDefaultAsync();
        }
        #endregion

        #region GetWarningCountAsync
        // This method retrieves the total count of warnings for a specific user in a guild.
        public async Task<int> GetWarningCountAsync(ulong guildId, ulong userId)
        {
            return await _dbContext.WarningData
                .CountAsync(w => w.GuildId == guildId && w.UserId == userId);
        }
        #endregion

        #region ClearWarningsForUserAsync
        // This method clears all warnings for a specific user in a guild.
        public async Task ClearWarningsForUserAsync(ulong guildId, ulong userId)
        {
            var warnings = await _dbContext.WarningData.Where(w => w.GuildId == guildId && w.UserId == userId).ToListAsync();
            _dbContext.WarningData.RemoveRange(warnings);
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region ClearWarningByIdAsync
        // This method clears a specific warning by its ID.
        public async Task ClearWarningByIdAsync(ulong guildId, int warningId)
        {
            var warning = await _dbContext.WarningData.Where(w => w.GuildId == guildId && w.WarningId == warningId).FirstOrDefaultAsync();
            if (warning != null)
            {
                _dbContext.WarningData.Remove(warning);
                await _dbContext.SaveChangesAsync();
            }
        }
        #endregion
    }
}
