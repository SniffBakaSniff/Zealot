using DSharpPlus.EventArgs;
using Zealot.Shared.Services;
using DSharpPlus;

namespace Zealot.Bot.Events
{
    public class MessageDeletedEvent() : IEventHandler<MessageDeletedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageDeletedEventArgs e)
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