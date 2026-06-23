using System.ComponentModel.DataAnnotations;
using Zealot.Shared.Enums;

namespace Zealot.Shared.Database.Models
{
    public class WarningEscalationRule
    {
        [Key]
        public ulong GuildId { get; set; }
        public int WarningCount { get; set; }
        public WarningEscalationType Punishment { get; set; }
        public int? DurationHours { get; set; }
    }
}