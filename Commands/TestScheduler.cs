using DSharpPlus.Commands;
using System.ComponentModel;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {

        [Command("test")]
        [Description("Test the task scheduler by adding a task for yourself after X minutes.")]
        public async Task TestSchedular(CommandContext ctx,
            [Description("When to execute the task. (X minutes from now)")] int delay)
        {
            var minutes = delay;
            var executeAtOffset = DateTimeOffset.UtcNow.AddMinutes(minutes);
            var executeAt = executeAtOffset.UtcDateTime;

            await _createTask.AddTaskAsync(
                taskType: "TestTask",
                guildId: ctx.Guild!.Id,
                userId: ctx.User.Id,
                executeAt: executeAt
            );

            await ctx.RespondAsync($"Scheduled a test task for <t:{executeAtOffset.ToUnixTimeSeconds()}:R>.");
        }
    }
}
