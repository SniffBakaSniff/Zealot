using System.ComponentModel.DataAnnotations;
using Zealot.Shared.Enums;

namespace Zealot.Shared.Database.Models
{
    public class WarningEscalationRule
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public int WarningCount { get; set; }
        public WarningEscalationType Punishment { get; set; }
        public Duration DurationHours { get; set; }
    }
}