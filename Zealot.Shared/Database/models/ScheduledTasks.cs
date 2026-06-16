public class ScheduledTasks
{
    public int Id { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime ExecuteAt { get; set; }
    public ulong? GuildId { get; set; } = null;
    public ulong? ChannelId { get; set; } = null;
    public ulong? UserId { get; set; } = null;
}