using System.ComponentModel;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace Zealot.Shared.Enums
{
    public enum WarningEscalationType
    {
        [ChoiceDisplayName("Timeout")]
        [Description("Timeout a user.")]
        timeout,
        [ChoiceDisplayName("Mute")]
        [Description("Mute a user.")]
        Mute,
        [ChoiceDisplayName("Kick")]
        [Description("Kick a user.")]
        kick,
        [ChoiceDisplayName("Ban")]
        [Description("Ban a user.")]
        ban
    }
}