using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace Zealot.Shared.Enums
{
    public enum CommandPermissions
    {
        [ChoiceDisplayName("Ban Members")]
        BanMembers,

        [ChoiceDisplayName("Unban Members")]
        UnbanMembers,

        [ChoiceDisplayName("Timeout Members")]
        TimeoutMembers,

        [ChoiceDisplayName("Untimeout Members")]
        RemoveTimeout,

        [ChoiceDisplayName("Kick Members")]
        KickMembers,

        [ChoiceDisplayName("Use Ping")]
        UsePing,

        [ChoiceDisplayName("Manage Prefix")]
        ManagePrefix,

        [ChoiceDisplayName("Purge Messages")]
        PurgeMessages,

        [ChoiceDisplayName("View Moderation Logs")]
        ViewModerationLogs,

        [ChoiceDisplayName("View Logs")]
        ViewLogs,

        [ChoiceDisplayName("Manage Configuration")]
        ManageConfiguration,

        [ChoiceDisplayName("View Warning Escalations")]
        ViewWarningEscalations,

        [ChoiceDisplayName("Manage Warning Escalations")]
        ManageWarningEscalations,

        [ChoiceDisplayName("Remove Warning Escalation")]
        RemoveWarningEscalation,

        [ChoiceDisplayName("View Warnings")]
        ViewWarnings,

        [ChoiceDisplayName("Warn Members")]
        WarnMembers,
    }
}