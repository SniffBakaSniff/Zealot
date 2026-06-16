
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

public enum TimeFrame
{
    [ChoiceDisplayName("Don't Delete Any")]
    None = 0,

    [ChoiceDisplayName("Previous Hour")]
    OneHour = 1,

    [ChoiceDisplayName("Previous 6 Hours")]
    SixHours = 6,

    [ChoiceDisplayName("Previous 12 Hours")]
    TwelveHours = 12,

    [ChoiceDisplayName("Previous 24 Hours")]
    TwentyFourHours = 24,

    [ChoiceDisplayName("Previous 3 Days")]
    SeventyTwoHours = 72,

    [ChoiceDisplayName("Previous 7 Days")]
    OneHundredSixtyEightHours = 168
}
