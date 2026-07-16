using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zealot.Shared.Database;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Enums;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Shared.Services
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        private readonly DiscordClient _client;
        //private readonly BotDbContext _dbContext;
        //private readonly IGuildSettingService _guildSettingService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

        public TaskSchedulerService(
            DiscordClient client,
           //BotDbContext dbContext,
           //IGuildSettingService guildSettingService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _client = client;
            //_dbContext = dbContext;
            //_guildSettingService = guildSettingService;
            _scopeFactory = serviceScopeFactory;
        }

        #region StartAsync
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var dbContext =
                        scope.ServiceProvider.GetRequiredService<BotDbContext>();

                    var now = DateTime.UtcNow;

                    var tasks = await dbContext.ScheduledTasks
                        .Where(task => task.ExecuteAt <= now)
                        .ToListAsync(cancellationToken);

                    foreach (var task in tasks)
                    {
                        try
                        {
                            await HandleTaskAsync(task);

                            dbContext.ScheduledTasks.Remove(task);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Error executing task {task.Id}: {ex.Message}");
                        }
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Scheduler loop error: {ex.Message}");
                }

                await Task.Delay(_pollInterval, cancellationToken);
            }
        }
        #endregion

        #region AddTaskAsync
        // Create Tasks that will get executed later
        public async Task AddTaskAsync(TaskType taskType, ulong guildId, ulong userId, DateTime executeAt)
        {
            // Create a new scope to get a new instance of the DbContext
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<BotDbContext>();

            var newTask = new ScheduledTasks
            {
                TaskType = taskType,
                GuildId = guildId,
                UserId = userId,
                ExecuteAt = executeAt,
            };

            var existing = await dbContext.ScheduledTasks.FirstOrDefaultAsync(x =>
                x.TaskType == taskType &&
                x.GuildId == guildId &&
                x.UserId == userId);

            if (existing is not null)
            {
                existing.ExecuteAt = executeAt;
            }
            
            dbContext.ScheduledTasks.Add(newTask);
            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region RemoveTaskAsync
        // Remove scheduled tasks
        public async Task RemoveTaskAsync(TaskType taskType, ulong guildId, ulong userId)
        {

            // Create a new scope to get new instances of the services
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<BotDbContext>();

            // Get any tasks matching the criteria
            var tasks = await dbContext.ScheduledTasks
                .Where(task =>
                    task.TaskType == taskType &&
                    task.GuildId == guildId &&
                    task.UserId == userId)
                .ToListAsync();

            // Delete them if they exist
            if (tasks.Any())
            {
                dbContext.ScheduledTasks.RemoveRange(tasks);
                await dbContext.SaveChangesAsync();
            }
        }
        #endregion

        #region HandleTaskAsync
        // Basic handler for scheduled tasks
        private async Task HandleTaskAsync(ScheduledTasks task)
        {
            // Create a new scope to get new instances of the services
            using var scope = _scopeFactory.CreateScope();
            var guildSettingService = scope.ServiceProvider
                .GetRequiredService<IGuildSettingService>();

            var guild = await _client.GetGuildAsync(task.GuildId!.Value);
            var user = await _client.GetUserAsync(task.UserId!.Value);
            switch (task.TaskType)
            {
                // Unbans a user. Might make it log the action.
                case TaskType.UnBan:
                    await guild.UnbanMemberAsync(user);
                    break;
            }
        }
        #endregion
    }
}
