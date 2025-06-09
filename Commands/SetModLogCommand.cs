using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

using Zealot.Attributes;

namespace Zealot.Commands
{
    // kinda a temp command ig 
    public partial class CommandsGroup
    {
        [Command("setlogchannel")]
        [Description("Sets the channel where moderation logs will be sent.")]
        [PermissionCheck("")]
        public async Task SetLogChannelAsync(CommandContext ctx,
            [Description("The channel to use for moderation logs.")] DiscordChannel channel)
        {
            // Set the channel in the database
            await _guildSettingService.SetModerationLogChannelAsync(ctx.Guild!.Id, channel.Id);

            // Build the embed
            var embed = new DiscordEmbedBuilder()
                .WithDescription($"Set moderation log channel to {channel.Mention}")
                .WithColor(DiscordColor.Gray);

            // Build the respons
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true);

            // Send the response
            await ctx.RespondAsync(response);
        }
    }
}