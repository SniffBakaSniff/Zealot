using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

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
    purge
}