using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Zealot.Shared;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Enums;

namespace Zealot.Bot.Commands
{
    public partial class CommandsGroup
    {
        [Command("warning_escalation")]
        [Description("Sets or updates a warning escalation rule.")]
        public async Task WarningEscalation(CommandContext ctx, 
            [Description("The amount of warnings.")]int warningCount, 
            [Description("The Punishment.")]WarningEscalationType escalationType, 
            [Description("How long the punishment should last.")]Duration duration = Duration.None, bool ephemeral = false)
        {   
            try
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Warning Escalation Rule Added")
                    .WithDescription("The warning escalation rule has been successfully created.")
                    .AddField("Warning Count", $"```{warningCount}```", true)
                    .AddField("Punishment", $"```{escalationType}```", true)
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);

                WarningEscalationRule rule = new()
                {
                    GuildId = ctx.Guild!.Id,
                    WarningCount = warningCount,
                    Punishment = escalationType,
                    DurationHours = duration
                };
                await _warningService.AddWarningEscalationRuleAsync(rule);

                if (duration != Duration.None)
                {
                    embed.AddField("Duration", $"```{duration}```", true);
                }

                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral(ephemeral);

                await ctx.RespondAsync(response);
            }
            catch(DuplicateWarningEscalationRuleException)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(
                    new DiscordEmbedBuilder()
                    .WithDescription($"There is already a rule for {warningCount} warnings.")
                    .WithColor(DiscordColor.Gray)).AsEphemeral());
            }
            catch(MaximumWarningEscalationRuleException ex)
            {
                await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(
                    new DiscordEmbedBuilder()
                    .WithDescription($"You’ve hit the limit of {ex.MaxAllowed} warning escalation rules.\nDelete an existing rule to create a new one.")
                    .WithColor(DiscordColor.Gray)).AsEphemeral());
            }
        }
    }
}