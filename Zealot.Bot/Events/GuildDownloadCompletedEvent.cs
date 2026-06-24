using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Events
{
    public class GuildDownloadCompletedEvent(IGuildDataService guildDataService, DiscordClient client) : IEventHandler<GuildDownloadCompletedEventArgs>
    {
        private static readonly CancellationTokenSource _cts = new();

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

            _ = StartStatusCycleAsync(client, _cts.Token);
        }
        
        #region StatusCycling
        private static async Task StartStatusCycleAsync(DiscordClient client, CancellationToken cancellationToken)
        {
            Log.Information("Status Cycling Initializing");
            var statuses = new[]
            {
                new DiscordActivity("Interpreting divine pings...", DiscordActivityType.Custom),
                new DiscordActivity("Wandering the debug desert", DiscordActivityType.Custom),
                new DiscordActivity("Smite first. Ask later.", DiscordActivityType.Custom),
                new DiscordActivity("Reading the Book of /help", DiscordActivityType.Custom),
                new DiscordActivity("Blessed by bugs 🐛", DiscordActivityType.Custom),
                new DiscordActivity("Zealot", DiscordActivityType.Custom),
                new DiscordActivity("Praying to the stack trace gods 🙏", DiscordActivityType.Custom),
                new DiscordActivity("Sacrificing RAM for wisdom", DiscordActivityType.Custom),
                new DiscordActivity("Logging sins", DiscordActivityType.Custom),
                new DiscordActivity("Judging your uptime ⏳", DiscordActivityType.Custom),
                new DiscordActivity("Performing miracles... slowly", DiscordActivityType.Custom),
                new DiscordActivity("Baptizing noobs", DiscordActivityType.Custom),
                new DiscordActivity("Excommunicating null references", DiscordActivityType.Custom),
                new DiscordActivity("Executing the sacred loop", DiscordActivityType.Custom),
                new DiscordActivity("Clerical errors: 0", DiscordActivityType.Custom),
                new DiscordActivity("Fasting... from updates", DiscordActivityType.Custom),
                new DiscordActivity("Communing with the API spirits", DiscordActivityType.Custom),
                new DiscordActivity("Channeling divine exception handling", DiscordActivityType.Custom),
                new DiscordActivity("Chanting async prayers", DiscordActivityType.Custom),
                new DiscordActivity("Summoning packets", DiscordActivityType.Custom),
                new DiscordActivity("Bearing witness to your logs", DiscordActivityType.Custom),
                new DiscordActivity("Awaiting prophecy via WebSocket", DiscordActivityType.Custom),
            };

            var rng = new Random();

            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var status = statuses[rng.Next(statuses.Length)];

                        await client.UpdateStatusAsync(
                            status,
                            DiscordUserStatus.Online);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to update status");
                    }

                    await Task.Delay(
                        TimeSpan.FromMinutes(10),
                        cancellationToken);
                }
            }, cancellationToken);
            Log.Information("Status Cycleing Initalized");
        }
        #endregion
    }
}