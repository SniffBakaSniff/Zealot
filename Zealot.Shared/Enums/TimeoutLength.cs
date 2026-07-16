using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace Zealot.Shared.Enums
{
    public enum TimeoutDuration
    {
        [ChoiceDisplayName("30 Minutes")]
        ThirtyMinutes = 0, // Handle separately as TimeSpan.FromMinutes(30)

        [ChoiceDisplayName("1 Hour")]
        OneHour = 1,

        [ChoiceDisplayName("3 Hours")]
        ThreeHours = 3,

        [ChoiceDisplayName("6 Hours")]
        SixHours = 6,

        [ChoiceDisplayName("12 Hours")]
        TwelveHours = 12,

        [ChoiceDisplayName("1 Day")]
        OneDay = 24,

        [ChoiceDisplayName("2 Days")]
        TwoDays = 48,

        [ChoiceDisplayName("3 Days")]
        ThreeDays = 72,

        [ChoiceDisplayName("5 Days")]
        FiveDays = 120,

        [ChoiceDisplayName("1 Week")]
        OneWeek = 168,

        [ChoiceDisplayName("2 Weeks")]
        TwoWeeks = 336,

        [ChoiceDisplayName("3 Weeks")]
        ThreeWeeks = 504,

        [ChoiceDisplayName("4 Weeks (Maximum)")]
        FourWeeks = 672
    }
}