using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nox.Core.Helpers;
using Nox.Messaging;
using Serilog;

namespace Samples.Listener;

internal class Program
{
    private static IConfiguration Configuration { get; set; } = null!;

    public static async Task<int> Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args);
        
        await hostBuilder.Build().RunAsync();
        return 0;
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
            .CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddNoxListeners();
            })
            .UseSerilog();
        
        return hostBuilder;
    }

}