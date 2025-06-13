using DSharpPlus;
using Zealot.Services;
using Zealot.Services.Interfaces;

namespace Zealot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService;
        private readonly IGuildSettingService _guildSettingService;
        private readonly TaskSchedulerService _taskScheduler;

        public CommandsGroup(
            IModerationLogService moderationLogService,
            IGuildSettingService guildSettingService,
            TaskSchedulerService taskScheduler
        )
        {
            _moderationLogService = moderationLogService;
            _guildSettingService = guildSettingService;
            _taskScheduler = taskScheduler;
        }
    }
}
