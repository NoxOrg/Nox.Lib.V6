
using DocumentFormat.OpenXml.InkML;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nox.Cli.Commands;
using Nox.Cli.Services;
using Nox.Dynamic;
using Nox.Dynamic.Configuration;
using Nox.Dynamic.Extensions;
using Serilog;
using Spectre.Console.Cli;
using System.Reflection;

internal class Program
{
    private static IConfiguration _configuration { get; set; } = null!;

    public static async Task<int> Main(string[] args)
    {

        var hostBuilder = CreateHostBuilder(args);


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
        // App Configuration

        _configuration = ConfigurationHelper.GetNoxConfiguration(args)!;

        // Logger

        ILogger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .CreateLogger();

        Log.Logger = logger;

        // HostBuilder

        var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(_configuration);
                services.AddDynamicDefinitionFeature();
                services.AddMessageBusFeature(_configuration);           
            })
            .UseSerilog();

        return hostBuilder;
    }


}




