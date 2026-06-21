using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildDownloadCompletedEvent(IGuildDataService guildDataService)
    {
        private readonly IGuildDataService _guildDataService = guildDataService;

        public static async Task GuildDownloadCompletedHandler(DiscordClient client, GuildDownloadCompletedEventArgs e, IGuildDataService guildDataService)
        {
            try
            {
                foreach (var guild in e.Guilds)
                {
                    await guildDataService.AddClientGuilds(guild.Value);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error Occured during: `GuildDownloadCompletedHandler`");
            }
        }
    }
}