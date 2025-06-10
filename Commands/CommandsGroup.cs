using DSharpPlus;
using Zealot.Services.Interfaces;

namespace Zealot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService;
        private readonly IGuildSettingService _guildSettingService;
        private readonly DiscordClient _client;

        public CommandsGroup(
            IModerationLogService moderationLogService,
            IGuildSettingService guildSettingService,
            DiscordClient client
        )
        {
            _client = client;
            _moderationLogService = moderationLogService;
            _guildSettingService = guildSettingService;
        }
    }
}
