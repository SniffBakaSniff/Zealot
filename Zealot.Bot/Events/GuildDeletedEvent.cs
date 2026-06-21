using DSharpPlus;
using DSharpPlus.EventArgs;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildDeletedEvent(IGuildDataService guildDataService)
    {
        private readonly IGuildDataService _guildDataService = guildDataService;

        // Handles the deletion of guild information in the database upon the triggering of the GuildDeletedEvent.
        public static async Task GuildDeletedHandler(DiscordClient client, GuildDeletedEventArgs e, IGuildDataService guildDataService)
        {
            await guildDataService.RemoveClientGuild(e.Guild.Id);
        }
    }
}