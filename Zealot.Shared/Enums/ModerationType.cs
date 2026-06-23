using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace Zealot.Shared.Enums
{
    // a enum for the moderation types
    public enum ModerationType
    {
        [ChoiceDisplayName("Ban")]
        ban,
        [ChoiceDisplayName("Temporary Ban")]
        tempBan,
        [ChoiceDisplayName("Unban")]
        unban,
        [ChoiceDisplayName("Mute")]
        mute,
        [ChoiceDisplayName("Temporary Mute")]
        tempMute,
        [ChoiceDisplayName("Unmute")]
        unmute,
        [ChoiceDisplayName("Kick")]
        kick,
        [ChoiceDisplayName("Purge")]
        purge,
        [ChoiceDisplayName("Warn")]
        warn,
        [ChoiceDisplayName("Remove Warning")]
        removeWarning,
        [ChoiceDisplayName("Timeout")]
        timeout
    }
}