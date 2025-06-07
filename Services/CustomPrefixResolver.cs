using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;

namespace Zealot.Services
{
    /// <summary>
    /// Resolves custom command prefixes for different Discord guilds.
    /// Implements DSharpPlus's IPrefixResolver interface to provide guild-specific command prefixes.
    /// </summary>
    public class CustomPrefixResolver : IPrefixResolver
    {

        /// <summary>
        /// Resolves the command prefix for a given message.
        /// Returns the length of the prefix if found, or -1 if no valid prefix is present.
        /// </summary>
        /// <param name="extension">The commands extension instance</param>
        /// <param name="message">The Discord message to check for a prefix</param>
        /// <returns>Length of the prefix if found; otherwise -1</returns>
        public async ValueTask<int> ResolvePrefixAsync(
            CommandsExtension extension,
            DiscordMessage message
        )
        {
            if (string.IsNullOrWhiteSpace(message.Content) || message.Channel is null)
            {
                return -1;
            }

            string[] prefixes = ["!", "~"]; // Placeholder until we have a method to set/get guild specific prefix

            foreach (var p in prefixes)
            {
                if (message.Content.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return p.Length;
                }
            }
            
            return -1;
        }
    }
}
