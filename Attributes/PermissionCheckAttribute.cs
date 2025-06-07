using Zealot.Services;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Serilog;

namespace Zealot.Attributes
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
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionCheckAttribute : ContextCheckAttribute
    {
        /// <summary>
        /// The unique identifier for the permission being checked.
        /// This key is used to look up permission settings in the guild's configuration.
        /// </summary>
        public string PermissionKey { get; }

        /// <summary>
        /// If true and no permissions are set (empty users and roles lists), all users can use the command.
        /// If false, the command requires explicit permission assignment.
        /// </summary>
        public bool UserBypass { get; }

        /// <summary>
        /// If true, only the bot developer(s) can use this command.
        /// This overrides all other permission checks.
        /// </summary>
        public bool DeveloperOnly { get; }

        /// <summary>
        /// List of developer user IDs that can use developer-only commands.
        /// </summary>
        public static readonly ulong[] DeveloperIds = [509585751487545345];

        /// <summary>
        /// Initializes a new instance of the PermissionCheckAttribute.
        /// </summary>
        /// <param name="permissionKey">The unique identifier for this permission check</param>
        /// <param name="userBypass">Whether the command is publicly available when no permissions are set</param>
        /// <param name="developerOnly">Whether only bot developers can use this command</param>
        public PermissionCheckAttribute(
            string permissionKey,
            bool userBypass = false,
            bool developerOnly = false
        )
        {
            PermissionKey = permissionKey;
            UserBypass = userBypass;
            DeveloperOnly = developerOnly;
        }
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
                attribute.PermissionKey
            );

            ulong userId = context.User.Id;

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

            var noPermissionResponse = new DiscordInteractionResponseBuilder()
                .WithContent("You do not have permission to do this!")
                .AsEphemeral(true);

            await context.RespondAsync(noPermissionResponse);

            return "You do not have permission to run this command.";
        }
    }
}
