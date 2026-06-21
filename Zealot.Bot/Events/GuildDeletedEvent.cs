using DSharpPlus;
using DSharpPlus.EventArgs;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildDeletedEvent(IGuildDataService guildDataService) : IEventHandler<GuildDeletedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, GuildDeletedEventArgs e)
        {
            await guildDataService.RemoveClientGuildAsync(e.Guild.Id);
        }
    }
}