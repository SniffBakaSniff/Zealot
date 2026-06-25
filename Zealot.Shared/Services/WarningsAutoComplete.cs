using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Zealot.Shared.Database;

namespace Zealot.Shared.Services
{
    // Uncertain of what a good spot for this would be so im leaving it in Zealot.Shared.Services for now.
    public class WarningsAutoComplete(BotDbContext dbContext) : IAutoCompleteProvider
    {
        public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        {
            // Get the current guild
            ulong guildId = context.Guild!.Id;
            // Get what the user has typed so far
            string input = context.UserInput ?? "";
            // Query database
            var rules = await dbContext.WarningEscalationRules
                .Where(x => x.GuildId == guildId)
                .OrderBy(x => x.WarningCount)
                .ToListAsync();

            return rules
                .Where(x => x.WarningCount.ToString().Contains(input))
                .Select(x => new DiscordAutoCompleteChoice(
                    $"{x.WarningCount} warnings → {x.Punishment}",
                    x.Id))
                .Take(25);
        }
    }
}