using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Zealot.Databases;

namespace Zealot.Services
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        private readonly DiscordClient _client;
        private readonly BotDbContext _dbContext;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

        public TaskSchedulerService(DiscordClient client, BotDbContext dbContext)
        {
            _client = client;
            _dbContext = dbContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    var tasks = await _dbContext.ScheduledTasks
                        .Where(task => task.ExecuteAt <= now)
                        .ToListAsync(cancellationToken);

                    foreach (var task in tasks)
                    {
                        try
                        {
                            await HandleTaskAsync(task);
                            _dbContext.ScheduledTasks.Remove(task);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing task {task.Id}: {ex.Message}");
                        }
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Scheduler loop error: {ex.Message}");
                }

                await Task.Delay(_pollInterval, cancellationToken);
            }
        }

        // Create Tasks that will get executed later
        public async Task AddTaskAsync(TaskType taskType, ulong guildId, ulong userId, DateTime executeAt)
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

        // Basic handler for scheduled tasks
        private async Task HandleTaskAsync(ScheduledTasks task)
        {
            var guild = await _client.GetGuildAsync(task.GuildId!.Value);
            var user = await _client.GetUserAsync(task.UserId!.Value);
            switch (task.TaskType)
            {
                // Unbans a user. Might make it log the action.
                case TaskType.UnBan:
                    await guild.UnbanMemberAsync(user);
                    break;

                case TaskType.UnMute:
                    ulong mutedRoleId = 1234567890; //This isnt implemented so putting placeholder
                    var mutedRole = await guild.GetRoleAsync(mutedRoleId);
                    var member = await guild.GetMemberAsync(user.Id);
                    await member.RevokeRoleAsync(mutedRole);
                    break;
            }
        }
    }
}
