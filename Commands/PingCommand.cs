using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

using Zealot.Attributes;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("ping")]
        [Description("Checks the bot's response time and uptime.")]
        [PermissionCheck("ping_command", userBypass: true)]
        public async Task PingAsync(
            CommandContext ctx,
            [Description("Send the response as ephemeral?")] bool ephemeral = true)
        {

            // Retrieve the bot's current latency for this guild and calculate uptime since launch.
            var latency = ctx.Client.GetConnectionLatency(ctx.Guild!.Id);
            var roundedLatency = Math.Round(latency.TotalMilliseconds);
            var uptime = DateTime.UtcNow - Program._botStartTime;

            // Build the embed displaying latency and uptime info.
            var embed = new DiscordEmbedBuilder()
                .WithTitle("🏓 Pong!")
                .AddField("Latency:", $"{roundedLatency} ms", true)
                .AddField("Uptime:", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m", true)
                .WithColor(DiscordColor.Cyan)
                .WithTimestamp(DateTime.UtcNow);

            // Send the embed response, optionally as ephemeral (visible only to the user).
            await ctx.RespondAsync(
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral(ephemeral)
            );
        }
    }
}
