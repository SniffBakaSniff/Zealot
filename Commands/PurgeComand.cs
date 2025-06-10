using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace Zealot.Commands
{
    public partial class CommandsGroup
    {
        [Command("purge")]
        [Description("Deletes a batch of messages with optional filters.")]
        public async Task PurgeCommand(CommandContext ctx,
            [Description("Number of messages to delete. (Max: 100)")] int amount,
            [Description("Only delete messages sent within the last X minutes.")] int? time = null,
            [Description("Only delete messages sent by this user.")] DiscordUser? user = null,
            [Description("Target a specific channel to purge messages from. Defaults to the current channel.")] DiscordChannel? channel = null,
            [Description("Reason for the purge.")] string? reason = null)
        {
            // Increment by 1 to exlude the command message
            if (ctx is TextCommandContext)
            {
                amount++;
            }

            // Notifies the user if the amount is outside the valid range (1–100).
            if (amount < 1 || amount > 100)
            {
                await ctx.RespondAsync("❌ Amount must be between 1 and 100.");
                return;
            }

            // Fallback to the current channel if none is provided.
            channel ??= ctx.Channel;
            user ??= null;
            var messages = new List<DiscordMessage>();

            // Fetches up to 100 of the most recent messages from the specified channel
            await foreach (var msg in channel.GetMessagesAsync(limit: 100))
            {
                messages.Add(msg);
            }

            // Filter messages based on optional user and time criteria,
            // and ensure they are not older than 14 days (Discord API limit).
            var now = DateTimeOffset.UtcNow;
            var filteredMessages = messages
                .Where(m =>
                    (user == null || m.Author!.Id == user.Id) &&
                    (!time.HasValue || (now - m.CreationTimestamp).TotalMinutes <= time.Value) &&
                    (now - m.CreationTimestamp).TotalDays < 14
                )
                .Take(amount)
                .ToList();

            // Inform the user if no messages match the filter criteria.
            if (filteredMessages.Count == 0)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("❌ No matching messages found to delete.")
                    .WithColor(DiscordColor.Gray);

                await ctx.RespondAsync(embed);
                return;
            }

            // Confirm the deletion with a temporary embed message.
            int totalMessages = filteredMessages.Count;

            // Decrement by 1 to exlude the command message
            if (ctx is TextCommandContext)
            {
                totalMessages--;
            }

            // Build the embed and send it
            var responseEmbed = new DiscordEmbedBuilder()
                .WithDescription($"✅ Deleted {totalMessages} message(s).")
                .WithColor(DiscordColor.Gray);

            // Log the purge
            await _moderationLogService.LogModeratorActionAsync(
                ctx.Guild!.Id,
                null,
                ctx.User.Id,
                ModerationType.purge.ToString(),
                embed: responseEmbed);

            // Delete the filtered messages from the channel.
            await channel.DeleteMessagesAsync(filteredMessages);
            
            await ctx.RespondAsync(responseEmbed);
            await Task.Delay(3000); // Wait 3 seconds before deleting the confirmation.
            await ctx.DeleteResponseAsync();
        }
    }
}