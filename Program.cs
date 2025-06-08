using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

using Zealot.Services;
using Zealot.Attributes;
using Zealot.Commands;
using Zealot.Services.Interfaces;
using Zealot.Databases;

namespace Zealot
{
    static class Program
    {
        // Get bot start time to calculate uptime for the ping command
        public static readonly DateTime _botStartTime = DateTime.UtcNow;
        private static readonly string LogFilePath = "Data/logs/log.txt";

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
                var dbContext = new BotDbContext();

                ConfigureServices(builder);
                ConfigureCommands(builder);
                ConfigureEventListeners(builder);
                ConfigureLogging();

                client = builder.Build();

                await PerformHealthCheck(dbContext);
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

        private static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception, "Unobserved task exception occurred.");
                eventArgs.SetObserved();
            };
        }

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

        private static DiscordClientBuilder CreateDiscordClientBuilder(string token)
        {
            return DiscordClientBuilder.CreateDefault(
                token,
                DiscordIntents.All
            );
        }

        private static void ConfigureServices(DiscordClientBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<BotDbContext>();
                services.AddScoped<IPrefixResolver, CustomPrefixResolver>();
                services.AddScoped<IModerationLogService, ModerationLogService>();
                services.AddScoped<IGuildSettingService, GuildSettingService>();

                // Add other essential services here
            });

        }

        private static void ConfigureCommands(DiscordClientBuilder builder)
        {
            builder.UseCommands(
                (_, extension) =>
                {
                    extension.AddCommands([typeof(CommandsGroup)]);

                    var textCommandProcessor = new TextCommandProcessor(new TextCommandConfiguration());
                    var slashCommandProcessor = new SlashCommandProcessor(new SlashCommandConfiguration());

                    extension.AddProcessors(textCommandProcessor);
                    extension.AddProcessor(slashCommandProcessor);
                    extension.AddCheck<PermissionCheck>();
                },
                new CommandsConfiguration
                {
                    RegisterDefaultCommandProcessors = true,
                    UseDefaultCommandErrorHandler = true,
                }
            );
        }

        private static void ConfigureEventListeners(DiscordClientBuilder builder)
        {
            builder.ConfigureEventHandlers(events =>
            {
                
            });
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                Log.Error(eventArgs.Exception, "Unobserved task exception occurred.");
                eventArgs.SetObserved();
            };
        }

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

            _ = StartStatusCycleAsync(client);
            Log.Information("Zealot is now running.");
            await Task.Delay(-1, cts.Token);
        }

        private static async Task StartStatusCycleAsync(DiscordClient client)
        {
            Log.Information("Status Cycleing Initalizing");
            var statuses = new[]
            {
                new DiscordActivity("Interpreting divine pings...", DiscordActivityType.Custom),
                new DiscordActivity("Wandering the debug desert", DiscordActivityType.Custom),
                new DiscordActivity("Smite first. Ask later.", DiscordActivityType.Custom),
                new DiscordActivity("Reading the Book of /help", DiscordActivityType.Custom),
                new DiscordActivity("Blessed by bugs 🐛", DiscordActivityType.Custom),
                new DiscordActivity("Zealot", DiscordActivityType.Custom),
                new DiscordActivity("Praying to the stack trace gods 🙏", DiscordActivityType.Custom),
                new DiscordActivity("Sacrificing RAM for wisdom", DiscordActivityType.Custom),
                new DiscordActivity("Logging sins", DiscordActivityType.Custom),
                new DiscordActivity("Judging your uptime ⏳", DiscordActivityType.Custom),
                new DiscordActivity("Performing miracles... slowly", DiscordActivityType.Custom),
                new DiscordActivity("Baptizing noobs", DiscordActivityType.Custom),
                new DiscordActivity("Excommunicating null references", DiscordActivityType.Custom),
                new DiscordActivity("Executing the sacred loop", DiscordActivityType.Custom),
                new DiscordActivity("Clerical errors: 0", DiscordActivityType.Custom),
                new DiscordActivity("Fasting... from updates", DiscordActivityType.Custom),
                new DiscordActivity("Communing with the API spirits", DiscordActivityType.Custom),
                new DiscordActivity("Channeling divine exception handling", DiscordActivityType.Custom),
                new DiscordActivity("Chanting async prayers", DiscordActivityType.Custom),
                new DiscordActivity("Summoning packets", DiscordActivityType.Custom),
                new DiscordActivity("Bearing witness to your logs", DiscordActivityType.Custom),
                new DiscordActivity("Awaiting prophecy via WebSocket", DiscordActivityType.Custom),
            };

            var rng = new Random();

            await Task.Run(async () =>
            {
                while (true)
                {
                    var status = statuses[rng.Next(statuses.Length)];
                    await client.UpdateStatusAsync(status, DiscordUserStatus.Online);
                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
            });
            Log.Information("Status Cycleing Initalized");
        }

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
    }
}
