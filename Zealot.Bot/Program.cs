using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

using Zealot.Shared.Services;
using Zealot.Bot.Attributes;
using Zealot.Bot.Commands;
using Zealot.Shared.Services.Interfaces;
using Zealot.Shared.Database;
using Zealot.Bot.Events;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using Npgsql;

namespace Zealot.Bot
{
    static class Program
    {
        // Get bot start time to calculate uptime for the ping command
        public static readonly DateTime _botStartTime = DateTime.UtcNow;
        private static readonly string LogFilePath = "Data/logs/log.txt";

        #region Task.Main
        public static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("Arguments detected. Bot will not start.");
                return;
            }

            ConfigureSerilog();
            DiscordClient? client = null;

            try
            {
                Log.Information("Starting Zealot...");
                var discordToken = ValidateDiscordToken();
                var builder = CreateDiscordClientBuilder(discordToken);
                //var dbContext = new BotDbContext();

                ConfigureServices(builder);
                ConfigureCommands(builder);
                ConfigureEventListeners(builder);

                client = builder.Build();

                //await PerformHealthCheck(dbContext);
                await StartBot(client);
            }
            catch (OperationCanceledException ex)
            {
                Log.Information(ex, "Shutdown signal received.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly.");
            }
            finally
            {
                await HandleShutdown(client);
            }
        }
        #endregion

        #region ConfigureSerilog
        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    LogFilePath,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception, "Unobserved task exception occurred.");
                eventArgs.SetObserved();
            };
        }
        #endregion

        #region ValidateToken
        private static string ValidateDiscordToken()
        {
            var discordToken = Environment.GetEnvironmentVariable("ZEALOT_TOKEN");
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                const string errorMessage = "No discord token found. Please provide a token via the ZEALOT_TOKEN environment variable.";
                Log.Fatal(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
            return discordToken;
        }
        #endregion

        #region ClientBuilder
        private static DiscordClientBuilder CreateDiscordClientBuilder(string token)
        {
            return DiscordClientBuilder.CreateDefault(
                token,
                DiscordIntents.All
            );
        }
        #endregion

        #region ConfigureServices
        private static void ConfigureServices(DiscordClientBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var botConnectionString =
                    "Host=localhost;Database=ZealotTest;Username=postgres;Password=Subaka1@;Maximum Pool Size=50;Minimum Pool Size=5;";
                    
                services.AddSingleton(sp =>
                {
                    var builder = new NpgsqlDataSourceBuilder(botConnectionString);

                    builder.EnableDynamicJson();

                    return new BotDataSource(builder.Build());
                });

                // Register the BodDbContext 
                services.AddDbContext<BotDbContext>((sp, options) =>
                {
                    options.UseNpgsql(
                        sp.GetRequiredService<BotDataSource>().DataSource);
                });

                services.AddScoped<IPrefixResolver, CustomPrefixResolver>();
                services.AddScoped<IModerationLogService, ModerationLogService>();
                services.AddScoped<IGuildSettingService, GuildSettingService>();
                services.AddSingleton<ITaskSchedulerService, TaskSchedulerService>();
                services.AddScoped<IWarningService, WarningService>();
                services.AddScoped<IAutoCompleteProvider, WarningsAutoComplete>();
                services.AddScoped<IGuildDataService, GuildDataService>();
                services.AddSingleton<ITimeoutService, TimeoutService>();

                services.AddLogging(logging =>
                {
                    logging.AddSerilog(Log.Logger, dispose: true);
                });

                // Add other essential services here
            });
        }
        #endregion

        #region ConfigureCommands
        private static void ConfigureCommands(DiscordClientBuilder builder)
        {
            builder.UseCommands(
                (_, extension) =>
                {
                    extension.AddCommands<BanCommands>();
                    extension.AddCommands<CommandPermissionsCommand>();
                    extension.AddCommands<SetupCommand>();
                    extension.AddCommands<KickCommand>();
                    extension.AddCommands<LogsCommands>();
                    extension.AddCommands<PingCommand>();
                    extension.AddCommands<PrefixCommand>();
                    extension.AddCommands<PurgeCommand>();
                    extension.AddCommands<SlashTimeoutCommands>();
                    extension.AddCommands<TextTimeoutCommands>();
                    extension.AddCommands<WarnCommand>();
                    extension.AddCommands<WarningEscalationCommand>();
                    extension.AddCommands<WarningsCommand>();

                    var textCommandProcessor = new TextCommandProcessor(new TextCommandConfiguration());
                    var slashCommandProcessor = new SlashCommandProcessor(
                        new SlashCommandConfiguration { UnconditionallyOverwriteCommands = true } // Fix slow startup times
                    );

                    extension.AddProcessors(textCommandProcessor);
                    extension.AddProcessor(slashCommandProcessor);
                    extension.AddCheck<PermissionCheck>();
                },
                new CommandsConfiguration
                {
                    RegisterDefaultCommandProcessors = true,
                    UseDefaultCommandErrorHandler = false,
                }
            );
        }
        #endregion

        #region EventHandler
        private static void ConfigureEventListeners(DiscordClientBuilder builder)
        {
            builder.ConfigureEventHandlers(events =>
            {
                events.AddEventHandlers<GuildCreatedEvent>(ServiceLifetime.Scoped);
                events.AddEventHandlers<GuildDeletedEvent>(ServiceLifetime.Scoped);
                events.AddEventHandlers<GuildDownloadCompletedEvent>(ServiceLifetime.Scoped);
                // Disabled for Now: events.AddEventHandlers<MessageCreatedEvent>(ServiceLifetime.Scoped);
                // Disabled for Now: events.AddEventHandlers<MessageUpdatedEvent>(ServiceLifetime.Scoped);
                // Disabled for Now: events.AddEventHandlers<MessageDeletedEvent>(ServiceLifetime.Scoped);
                // Disabled for Now: events.AddEventHandlers<MessageReactionAddedEvent>(ServiceLifetime.Scoped);
                // Disabled for Now: events.AddEventHandlers<MessageReactionRemovedEvent>(ServiceLifetime.Scoped);
            });
        }
        #endregion

        #region DbHealthCheck
        private static async Task PerformHealthCheck(BotDbContext dbContext)
        {
            try
            {
                await dbContext.Database.CanConnectAsync();
                Log.Information("Database connection test successful");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to connect to database");
            }
        }
        #endregion

        #region StartBot
        private static async Task StartBot(DiscordClient client)
        {
            var status = new DiscordActivity("Zealot", DiscordActivityType.Custom);
            await client.ConnectAsync(status, DiscordUserStatus.Online);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            await Task.Delay(1000);
      
            var services = client.ServiceProvider!;
            var scheduler = services.GetRequiredService<ITaskSchedulerService>();
            _ = scheduler.StartAsync(cts.Token);
            Log.Information("Zealot is now running.");
            await Task.Delay(-1, cts.Token);
        }
        #endregion

        #region HandleShutdown
        private static async Task HandleShutdown(DiscordClient? client)
        {
            if (client != null)
            {
                try
                {
                    await client.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while disconnecting the Discord client.");
                }
                finally
                {
                    client.Dispose();
                }
            }
            Log.Warning("Zealot is shutting down... closing and flushing logs.");
            await Log.CloseAndFlushAsync();
        }
        #endregion
    }
}
