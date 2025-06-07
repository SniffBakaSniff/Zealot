using System.ComponentModel.DataAnnotations;

namespace Zealot.Database.Models;
public class GuildSettings
{
    [Key]
    public ulong GuildId { get; set; }
    public string Prefix { get; set; } = "!";
}