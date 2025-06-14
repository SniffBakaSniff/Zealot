using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("config")]
        [Description("View or update your server’s moderation settings.")]
        [RequirePermissions(DiscordPermission.Administrator)]
        public async Task ConfigCommand(
            CommandContext ctx,
            [Description("The channel where moderator logs will be sent.")] DiscordChannel? channel = null,
            [Description("The role that will be used for muting users.")] DiscordRole? role = null)
        {
            ulong guildId = ctx.Guild!.Id;

            // Fetch existing settings
            ulong? currentChannelId = await _guildSettingService.GetModerationLogChannelAsync(guildId);
            ulong? currentRoleId = await _guildSettingService.GetMutedRoleIdAsync(guildId);

            bool updated = false;

            // Update settings if provided
            if (channel is not null)
            {
                await _guildSettingService.SetModerationLogChannelAsync(guildId, channel.Id);
                currentChannelId = channel.Id;
                updated = true;
            }

            if (role is not null)
            {
                await _guildSettingService.SetMutedRoleIdAsync(guildId, role.Id);
                currentRoleId = role.Id;
                updated = true;
            }

            // Build the embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Configuration Settings")
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter($"Requested by {ctx.User.Username}", ctx.User.AvatarUrl);

            embed.AddField("Mod Logs Channel",
                currentChannelId is null ? "*Not Set*" : $"<#{currentChannelId}>",
                inline: true);

            embed.AddField("Muted Role",
                currentRoleId is null ? "*Not Set*" : $"<@&{currentRoleId}>",
                inline: true);

            if (updated)
                embed.WithDescription("✅ Settings have been updated.");

            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed);

            await ctx.RespondAsync(response);
        }
    }
}
