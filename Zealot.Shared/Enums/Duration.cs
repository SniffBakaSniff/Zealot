using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
namespace Zealot.Shared.Enums
{
    public enum Duration
    {
        [ChoiceDisplayName("None")]
        None = 0,

        [ChoiceDisplayName("One Hour")]
        OneHour = 1,

        [ChoiceDisplayName("Six Hours")]
        SixHours = 6,

        [ChoiceDisplayName("Twelve Hours")]
        TwelveHours = 12,

        [ChoiceDisplayName("One Day")]
        TwentyFourHours = 24,

        [ChoiceDisplayName("Three Days")]
        SeventyTwoHours = 72,

        [ChoiceDisplayName("Seven Days")]
        OneHundredSixtyEightHours = 168
    }
}