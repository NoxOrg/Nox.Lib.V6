using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nox.Core.Configuration;
using Nox.Messaging;
using Samples.Cli.Commands;
using Samples.Cli.Services;
using Serilog;
using Spectre.Console.Cli;

namespace Samples.Cli;

internal class Program
{
    private static IConfiguration Configuration { get; set; } = null!;

    public static async Task<int> Main(string[] args)
    {

        var hostBuilder = CreateHostBuilder(args);
//        var host = hostBuilder.Build();
        
        
        // MassTransit doesn't like Spectre Executor
        if (args.Length > 0 && args[0].Equals("listen", StringComparison.OrdinalIgnoreCase))
        {
            await hostBuilder.Build().RunAsync();
            return 0;
        }

        var registrar = new TypeRegistrar(hostBuilder);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            // Register available commands

            config.AddCommand<SyncCommand>("sync")
                .WithDescription("Builds database and syncs data.")
                .WithExample(new[] { "sync" });

        });

        return await app.RunAsync(args); 

    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        Configuration = ConfigurationHelper.GetNoxAppSettings(args)!;
        // Logger

        ILogger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();

        Log.Logger = logger;
       
        // HostBuilder
        var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddNoxMessaging(busConsumers =>
                {
                    busConsumers.AddConsumer<CountryCreatedEventConsumer>();
                });
            })
            .UseSerilog();

        return hostBuilder;
    }

}