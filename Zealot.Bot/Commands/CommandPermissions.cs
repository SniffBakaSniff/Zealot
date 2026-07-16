using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Zealot.Bot.Attributes;
using Zealot.Shared.Enums;

namespace Zealot.Bot.Commands
{
    public class CommandPermissionsCommand
    {
        [Command("permissions")]
        [Description("View or update your server’s moderation settings.")]
        [PermissionCheck(CommandPermissions.ManageConfiguration, defaultPermission: DiscordPermission.ModerateMembers)]
        public async Task ConfigCommandPermissions(
            SlashCommandContext ctx,
            CommandPermissions permissions)
        {            
            // Build the embed
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Placeholder.")
                .WithColor(DiscordColor.Gray)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter($"Requested by {ctx.User.Username}", ctx.User.AvatarUrl);

            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed);

            await ctx.RespondAsync(response);
        }
    }
}
