using Zealot.Databases;

namespace Zealot.Services
{
    public class CreateTask
    {
        private readonly BotDbContext _dbContext;

        public CreateTask(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // basic implemenation for creating a new scheduled Task
        public async Task AddTaskAsync(string taskType, ulong guildId, ulong userId, DateTime executeAt)
        {
            var newTask = new ScheduledTasks
            {
                TaskType = taskType,
                GuildId = guildId,
                UserId = userId,
                ExecuteAt = executeAt,
            };

            _dbContext.ScheduledTasks.Add(newTask);
            await _dbContext.SaveChangesAsync();
        }

    }
}