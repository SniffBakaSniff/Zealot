using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;

using Zealot.Services.Interfaces;

namespace Zealot.Services
{
    /// <summary>
    /// Resolves custom command prefixes for different Discord guilds.
    /// Implements DSharpPlus's IPrefixResolver interface to provide guild-specific command prefixes.
    /// </summary>
    public class CustomPrefixResolver : IPrefixResolver
    {
        private readonly IGuildSettingService _guildSettingsService;

        /// <summary>
        /// Initializes a new instance of the CustomPrefixResolver.
        /// </summary>
        /// <param name="guildSettingsService">Service for accessing guild-specific settings</param>
        public CustomPrefixResolver(IGuildSettingService guildSettingsService)
        {
            _guildSettingsService = guildSettingsService;
        }

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

            if (message.Channel.GuildId.HasValue)
            {
                var guildId = message.Channel.GuildId.Value;
                var prefix = await _guildSettingsService.GetGuildPrefixAsync(guildId);
                if (message.Content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return prefix.Length;
                }
            }

            return -1;
        }
    }
}