using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;
using Zealot.Shared.Services.Interfaces;

namespace Zealot.Bot.Commands
{
    public class SetupCommand(IGuildSettingService guildSettingService)
    {
        [Command("setup")]
        [Description("View or update your server’s moderation settings.")]
        [PermissionCheck(CommandPermissions.ManageConfiguration, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task SetupCommandAsync(
            SlashCommandContext ctx,
            [Description("The channel where moderator logs will be sent.")] DiscordChannel? channel = null)
        {
            ulong guildId = ctx.Guild!.Id;

            // Fetch existing settings
            ulong? currentChannelId = await guildSettingService.GetModerationLogChannelAsync(guildId);

            bool updated = false;

            // Update settings if provided
            if (channel is not null)
            {
                await guildSettingService.SetModerationLogChannelAsync(guildId, channel.Id);
                currentChannelId = channel.Id;
                updated = true;
            }

            // Build the embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Server Setup")
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter($"Requested by {ctx.User.Username}", ctx.User.AvatarUrl);

            embed.WithDescription(updated
                ? "The server configuration has been updated."
                : "Current server configuration.");

            embed.AddField(
                "Moderation Log Channel",
                currentChannelId is null
                    ? "⚠️ **Not configured**\nModeration actions will not be logged."
                    : $"<#{currentChannelId}>",
                false);

            embed.AddField(
                "How to Change",
                $"Run {Formatter.InlineCode("/setup")} and select a channel.",
                false);

            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed);

            await ctx.RespondAsync(response);
        }
    }
}
