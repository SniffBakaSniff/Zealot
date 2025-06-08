using Zealot.Services.Interfaces;

namespace Zealot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService;
        private readonly IGuildSettingService _guildSettingService;

        public CommandsGroup(
            IModerationLogService moderationLogService,
            IGuildSettingService guildSettingService
        )
        {
            _moderationLogService = moderationLogService;
            _guildSettingService = guildSettingService;
        }

    }
}