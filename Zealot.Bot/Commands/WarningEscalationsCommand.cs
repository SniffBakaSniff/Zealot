using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        [Command("view_warning_escalations")]
        [Description("Sets or updates a warning escalation rule.")]
        public async Task WarningEscalations(CommandContext ctx, bool ephemeral = false)
        {   
            try
            {
                List<WarningEscalationRule> escalationRules = await _warningService.GetWarningEscalationRulesAsync(ctx.Guild!.Id);

                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Warning Escalation Rule Added")
                    .WithDescription("The warning escalation rule has been successfully created.")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                foreach (var rule in escalationRules)
                {
                    int warningCount = rule.WarningCount;
                    WarningEscalationType escalationType = rule.Punishment;
                    Duration duration = rule.DurationHours;

                    embed.AddField("Warning Count", $"```{warningCount}```", true);
                    embed.AddField("Punishment", $"```{escalationType}```", true);
                    embed.AddField("Duration", $"```{duration}```", true);
                }

                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral(ephemeral);

                await ctx.RespondAsync(response);
            }
            catch
            {
                // No Error Response
            }
        }
    }
}