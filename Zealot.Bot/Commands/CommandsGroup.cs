using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup(
        IModerationLogService moderationLogService,
        IGuildSettingService guildSettingService,
        ITaskSchedulerService taskSchedulerService,
        IWarningService warningService
        )
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService = moderationLogService;
        private readonly IGuildSettingService _guildSettingService = guildSettingService;
        private readonly ITaskSchedulerService _taskSchedulerService = taskSchedulerService;
        private readonly IWarningService _warningService = warningService;
    }
}
