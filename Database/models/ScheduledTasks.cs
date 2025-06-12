public class ScheduledTasks
{
    public int Id { get; set; }
    public string TaskType { get; set; } = null!;
    public DateTime ExecuteAt { get; set; }
    public ulong? GuildId { get; set; } = null;
    public ulong? ChannelId { get; set; } = null;
    public ulong? UserId { get; set; } = null;
}