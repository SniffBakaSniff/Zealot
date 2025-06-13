public interface ITaskSchedulerService
{
    /// <summary>
    /// Starts the background task scheduler.
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation of the scheduler.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new scheduled task to be executed at a specific time.
    /// </summary>
    /// <param name="taskType">The type of task to schedule (e.g., "TempBan", "TempMute").</param>
    /// <param name="guildId">The ID of the guild where the task is relevant.</param>
    /// <param name="userId">The ID of the user the task is targeting.</param>
    /// <param name="executeAt">The date and time when the task should execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddTaskAsync(string taskType, ulong guildId, ulong userId, DateTime executeAt);
}