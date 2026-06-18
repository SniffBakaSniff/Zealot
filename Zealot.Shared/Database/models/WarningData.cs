using System.ComponentModel.DataAnnotations;

namespace Zealot.Shared.Database.Models
{
    public class WarningData
    {
        [Key]
        public int Id { get; set; }
        public int WarningId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong ModeratorId { get; set; }
        [MaxLength(2000)]
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}