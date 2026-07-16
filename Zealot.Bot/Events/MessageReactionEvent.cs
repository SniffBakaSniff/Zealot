using DSharpPlus.EventArgs;
using Zealot.Shared.Services;
using DSharpPlus;

namespace Zealot.Bot.Events
{
    public class MessageReactionAddedEvent() : IEventHandler<MessageReactionAddedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs e)
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

    public class MessageReactionRemovedEvent() : IEventHandler<MessageReactionRemovedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageReactionRemovedEventArgs e)
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