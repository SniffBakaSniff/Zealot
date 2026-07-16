using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using Zealot.Shared.Services;

namespace Zealot.Bot.Events
{
    public class MessageCreatedEvent() : IEventHandler<MessageCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs e)
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