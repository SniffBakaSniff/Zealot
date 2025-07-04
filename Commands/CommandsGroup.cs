using Zealot.Services.Interfaces;

namespace Zealot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService;
        private readonly IGuildSettingService _guildSettingService;
        private readonly ITaskSchedulerService _taskSchedulerService;

        public CommandsGroup(
            IModerationLogService moderationLogService,
            IGuildSettingService guildSettingService,
            ITaskSchedulerService taskSchedulerService
        )
        {
            _moderationLogService = moderationLogService;
            _guildSettingService = guildSettingService;
            _taskSchedulerService = taskSchedulerService;
        }
    }
}
