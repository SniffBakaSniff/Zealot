using DSharpPlus.EventArgs;
using Zealot.Shared.Services;
using DSharpPlus;

namespace Zealot.Bot.Events
{
    public class MessageUpdatedEvent() : IEventHandler<MessageUpdatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageUpdatedEventArgs e)
        {
            try
            {
                // Do nothing.
            }
            catch
            {
                // Do nothing.
            }
        }
    }
}