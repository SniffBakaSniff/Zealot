using Zealot.Shared.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Serilog;
using Zealot.Shared.Enums;

// Can be Modified in the future for fully customizable per-server permission handilng.
namespace Zealot.Bot.Attributes
{
    /// <summary>
    /// Attribute for checking command permissions in a Discord guild.
    /// This attribute works in conjunction with the PermissionCheck class to control access to commands.
    /// </summary>
    /// <remarks>
    /// Usage example:
    /// [PermissionCheck("ping_command", userBypass: true)]
    /// public async Task PingCommand(CommandContext ctx) { }
    ///
    /// For developer-only commands:
    /// [PermissionCheck("admin_command", developerOnly: true)]
    /// public async Task AdminCommand(CommandContext ctx) { }
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the PermissionCheckAttribute.
    /// </remarks>
    /// <param name="permissionKey">The unique identifier for this permission check</param>
    /// <param name="userBypass">Whether the command is publicly available when no permissions are set</param>
    /// <param name="developerOnly">Whether only bot developers can use this command</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionCheckAttribute(
        CommandPermissions permissionKey,
        bool userBypass = false,
        bool developerOnly = false,
        DiscordPermission defaultPermission = DiscordPermission.ModerateMembers
        ) : ContextCheckAttribute
    {
        public CommandPermissions PermissionKey { get; } = permissionKey;
        public bool UserBypass { get; } = userBypass;
        public bool DeveloperOnly { get; } = developerOnly;
        public DiscordPermission DefaultPermission = DiscordPermission.ModerateMembers;
        public static readonly ulong[] DeveloperIds = [509585751487545345];
    }

    /// <summary>
    /// Implements the permission check logic for the PermissionCheckAttribute.
    /// This class evaluates whether a user has permission to execute a command based on:
    /// - Administrator status (if bypass enabled)
    /// - Direct user permissions
    /// - Role-based permissions
    /// - Command enabled/disabled status
    /// </summary>
    public class PermissionCheck : IContextCheck<PermissionCheckAttribute>
    {
        /// <summary>
        /// Executes the permission check for a command.
        /// </summary>
        /// <param name="attribute">The PermissionCheckAttribute containing the check configuration</param>
        /// <param name="context">The command context containing information about the command execution</param>
        /// <returns>
        /// - null if the check passes and the command should execute
        /// - An error message string if the check fails
        /// </returns>
        public async ValueTask<string?> ExecuteCheckAsync(
            PermissionCheckAttribute attribute,
            CommandContext context
        )
        {

            // Log the start of the permission check
            Log.Information(
                "Permission check started for command '{PermissionKey}' in guild {GuildId} by user {UserId}.",
                attribute.PermissionKey, context.Guild?.Id, context.User.Id
            );

            ulong userId = context.User.Id;

            // Check if UserBypass
            if (attribute.UserBypass)
            {
                return null;
            }

            // Check if this is a developer-only command
            if (attribute.DeveloperOnly)
            {
                // Check if the user is a developer
                if (PermissionCheckAttribute.DeveloperIds.Contains(userId))
                {
                    Log.Information(
                        "User {UserId} is a developer. Allowing access to developer-only command '{PermissionKey}'.",
                        userId,
                        attribute.PermissionKey
                    );
                    return null;
                }
                else
                {
                    Log.Warning(
                        "User {UserId} attempted to use developer-only command '{PermissionKey}' but is not a developer.",
                        userId,
                        attribute.PermissionKey
                    );

                    string devOnlyMessage = "Dev Only";

                    var devOnlyResponse = new DiscordInteractionResponseBuilder()
                        .WithContent(devOnlyMessage)
                        .AsEphemeral(true);

                    await context.RespondAsync(devOnlyResponse);

                    return "This command is only available to bot developers.";
                }
            }

            // Check administrator bypass
            if (context.Member!.Permissions.HasPermission(DiscordPermission.Administrator))
            {
                Log.Information(
                    "User {UserId} has Administrator permission. Bypassing permission check for command '{PermissionKey}'.",
                    userId,
                    attribute.PermissionKey
                );
                return null;
            }

            // Check defaultPermissionCheck
            if (context.Member!.Permissions.HasPermission(attribute.DefaultPermission))
            {
                return null;
            }

            var noPermissionResponse = new DiscordInteractionResponseBuilder()
                .WithContent("You do not have permission to do this!")
                .AsEphemeral(true);

            await context.RespondAsync(noPermissionResponse);

            return "You do not have permission to run this command.";
        }
    }
}
