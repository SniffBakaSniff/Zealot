using DSharpPlus;
using DSharpPlus.EventArgs;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildCreatedEvent(IGuildDataService guildDataService)
    {
        private readonly IGuildDataService _guildDataService = guildDataService;

        // Handles the addition of guild information in the database upon the triggering of the GuildCreatedEvent.
        public static async Task GuildCreatedHandler(DiscordClient client, GuildCreatedEventArgs e, IGuildDataService guildDataService)
        {
            await guildDataService.AddClientGuildsAsync(e.Guild);
        }
    }
}