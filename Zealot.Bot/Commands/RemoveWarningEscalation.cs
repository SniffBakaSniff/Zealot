using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Zealot.Shared.Services;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        [Command("remove_warning_escalation")]
        public async Task RemoveWarningEscalationAsync(
            CommandContext ctx,
            [SlashAutoCompleteProvider(typeof(WarningsAutoComplete))]
            int ruleId)
        {
            var rule = await _warningService.GetWarningEscalationRuleByIdAsync(ruleId);
            await _warningService.ClearWarningEscalationAsync(ruleId);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Warning Escalation Removed")
                .WithDescription("The warning escalation rule has been successfully removed.")
                .AddField("Warning Count", $"```{rule.WarningCount}```", true)
                .AddField("Punishment", $"```{rule.Punishment}```", true)
                .AddField("Duration", $"```{rule.DurationHours}```", true)
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTime.UtcNow);

            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral();

            await ctx.RespondAsync(response);
        }
    }
}