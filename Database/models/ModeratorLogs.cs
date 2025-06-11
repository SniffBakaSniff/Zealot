using System.ComponentModel.DataAnnotations;

namespace Zealot.Database.Models;

public class ModeratorLogs
{
    [Key]
    public int Id { get; set; }

    [Required]
    public ulong GuildId { get; set; }

    public ulong? UserId { get; set; }

    [Required]
    public ulong ModeratorId { get; set; }

    [Required]
    public string ActionType { get; set; } = string.Empty; // e.g. "ban", "warn", etc.

    public string? Reason { get; set; }

    public TimeSpan? Duration { get; set; } // Nullable for permanent actions

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? ContextMessage { get; set; } // Change this to be the BYTEA for the DiscordAttachment

    [Required]
    public int CaseNumber { get; set; }
}

// Maybe find a better spot for this
public class ModeratorLogsDTO
{
    public ulong? UserId { get; set; }
    public ulong ModeratorId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CaseNumber { get; set; }
}