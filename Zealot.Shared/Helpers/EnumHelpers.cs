using System.Reflection;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

public static class EnumExtensions
{
    public static string GetChoiceDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        if (member == null)
            return value.ToString();

        var attribute = member.GetCustomAttribute<ChoiceDisplayNameAttribute>();

        return attribute?.DisplayName ?? value.ToString();
    }
}