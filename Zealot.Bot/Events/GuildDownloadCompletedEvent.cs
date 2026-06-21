using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildDownloadCompletedEvent(IGuildDataService guildDataService) : IEventHandler<GuildDownloadCompletedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            try
            {
                foreach (var guild in e.Guilds)
                {
                    await guildDataService.AddClientGuildsAsync(guild.Value);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error Occured during: `GuildDownloadCompletedHandler`");
            }
        }
    }
}