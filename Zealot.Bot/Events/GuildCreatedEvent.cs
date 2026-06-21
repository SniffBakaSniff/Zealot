using DSharpPlus;
using DSharpPlus.EventArgs;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildCreatedEvent(IGuildDataService guildDataService) : IEventHandler<GuildCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, GuildCreatedEventArgs e)
        {
            await guildDataService.AddClientGuildsAsync(e.Guild);
        }
    }
}