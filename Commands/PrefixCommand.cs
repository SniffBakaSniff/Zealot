using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("prefix")]
        [Description("Sets the command prefix for the bot.")]
        public async Task prefix(CommandContext ctx,
        [Description("The new prefix for the bot. (e.g. `!` or `~`)")] string? prefix = null)
        {
            ulong guildId = ctx.Guild!.Id;

            // Send a warning if the prefix is too long
            if (prefix?.Length > 3)
            {
                // Build the embed
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("❌ The prefix is too long. It must be between 1 and 3 characters. (e.g., **`~!`**)")
                    .WithColor(DiscordColor.Gray);

                // Build the response
                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral(true);

                // Send the response
                await ctx.RespondAsync(response);
            }

                // If no prefix is provided then get the current prefix
                if (prefix is null)
                {
                    // Get the prefix
                    prefix = await _guildSettingService.GetGuildPrefixAsync(guildId);

                    // Build the emebed
                    var embed = new DiscordEmbedBuilder()
                        .WithDescription($"The Current prefix is **`{prefix}`**")
                        .WithColor(DiscordColor.Gray);

                    // Build the repsonse
                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed);

                    // Send the response
                    await ctx.RespondAsync(response);
                }
                else
                {
                    // Set the prefix in the database
                    await _guildSettingService.SetGuildPrefixAsync(guildId, prefix);

                    // Build the embed
                    var embed = new DiscordEmbedBuilder()
                        .WithDescription($"prefix has been updated to **`{prefix}`**")
                        .WithColor(DiscordColor.Gray);
                        
                    // Build the repsponse 
                var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed);

                    // Send the response
                    await ctx.RespondAsync(response);
                }
        }
    }
}