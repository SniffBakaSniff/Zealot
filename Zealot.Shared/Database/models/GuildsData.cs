using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Zealot.Shared.Database.Models
{
    public class GuildsData
    {
        [Key]
        public ulong GuildId { get; set; }
        public required string Name { get; set; }
        public string? IconUrl { get; set; }
        public int TotalMembers { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
