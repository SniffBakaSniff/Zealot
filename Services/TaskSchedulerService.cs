using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Zealot.Databases;

namespace Zealot.Services
{
    public class TaskSchedulerService
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

        // Basic handler for scheduled tasks
        private async Task HandleTaskAsync(ScheduledTasks task)
        {
            switch (task.TaskType)
            {
                case "TestTask":
                    var user = await _client.GetUserAsync(task.UserId!.Value);
                    await user.SendMessageAsync("This is a task that's been run.");
                    break;
            }
        }
    }
}
