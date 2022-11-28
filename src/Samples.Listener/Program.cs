using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nox.Messaging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
    .ConfigureServices((hostContext, services) =>
    {
        var env = hostContext.HostingEnvironment;
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, false)
            .Build();
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        Log.Logger = logger;
        services.AddMessageBus(config);
    })
    .UseSerilog();
