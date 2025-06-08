using Zealot.Services.Interfaces;

namespace Zealot.Commands
{
    // The command group for all the commands
    public partial class CommandsGroup
    {
        // handle dependency injection here
        private readonly IModerationLogService _moderationLogService;

        public CommandsGroup(
            IModerationLogService moderationLogService
        )
        {
            _moderationLogService = moderationLogService;
        }
    }
}