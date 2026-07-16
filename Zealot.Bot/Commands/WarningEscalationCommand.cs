using System.ComponentModel;
using System.Reflection;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared;
using Zealot.Shared.Database.Models;
using Zealot.Shared.Enums;
using Zealot.Shared.Services;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    [Command("escalations")]
    [Description("Manage warning auto escalation rules.")]
    public class WarningEscalationCommand(IWarningService warningService)
    {
        [Command("add")]
        [Description("Sets or updates a warning escalation rule.")]
        [PermissionCheck(CommandPermissions.ManageWarningEscalations, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task AddWarningEscalationAsync(SlashCommandContext ctx, 
            [Description("The amount of warnings.")]int warningCount, 
            [Description("The Punishment.")]WarningEscalationType escalationType, 
            [Description("How long the punishment should last.")]Duration duration = Duration.None)
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
                await warningService.AddWarningEscalationRuleAsync(rule);

                if (duration != Duration.None)
                {
                    embed.AddField("Duration", $"```{duration.GetChoiceDisplayName()}```", true);
                }

                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed);

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

        [Command("remove")]
        [Description("Remove an escalation rule.")]
        [PermissionCheck(CommandPermissions.RemoveWarningEscalation, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task RemoveWarningEscalationAsync(
            SlashCommandContext ctx,
            [SlashAutoCompleteProvider(typeof(WarningsAutoComplete))]
            int ruleId)
        {
            var rule = await warningService.GetWarningEscalationRuleByIdAsync(ruleId);
            await warningService.ClearWarningEscalationAsync(ruleId);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Warning Escalation Removed")
                .WithDescription("The warning escalation rule has been successfully removed.")
                .AddField("Warning Count", $"```{rule.WarningCount}```", true)
                .AddField("Punishment", $"```{rule.Punishment}```", true)
                .AddField("Duration", $"```{rule.DurationHours.GetChoiceDisplayName()}```", true)
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTime.UtcNow);
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral();
            await ctx.RespondAsync(response);
        }

        [Command("view")]
        [Description("Sets or updates a warning escalation rule.")]
        [PermissionCheck(CommandPermissions.ViewWarningEscalations, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task ViewWarningEscalationsAsync(SlashCommandContext ctx)
        {   
            try
            {
                List<WarningEscalationRule> escalationRules = await warningService.GetWarningEscalationRulesAsync(ctx.Guild!.Id);
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Warning Escalation Rules")
                    //.WithDescription("The warning escalation rule has been successfully created.")
                    .WithColor(DiscordColor.Gray)
                    .WithTimestamp(DateTime.UtcNow);
                foreach (var rule in escalationRules)
                {
                    int warningCount = rule.WarningCount;
                    WarningEscalationType escalationType = rule.Punishment;
                    Duration duration = rule.DurationHours;
                    embed.AddField("Warning Count", $"```{warningCount}```", true);
                    embed.AddField("Punishment", $"```{escalationType}```", true);
                    embed.AddField("Duration", $"```{duration.GetChoiceDisplayName()}```", true);
                }
                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed);
                await ctx.RespondAsync(response);
            }
            catch
            {
                // No Error Response
            }
        }
    }
}