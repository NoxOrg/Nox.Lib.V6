using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace NoxConsole.Configuration;

public class ConfigurationHelper
{
    public static IConfiguration GetApplicationConfiguration(string[] args)
    {
        var pathToContentRoot = Directory.GetCurrentDirectory();
        var json = Path.Combine(pathToContentRoot, "appsettings.json");

        if (!File.Exists(json))
        {
            var pathToExe = Process.GetCurrentProcess()?.MainModule?.FileName;
            pathToContentRoot = Path.GetDirectoryName(pathToExe);
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(pathToContentRoot)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        if (IsDevelopment())
        {
            builder.AddUserSecrets<ConfigurationHelper>();
        }

        return builder.Build();

    }

    private static bool IsDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ENVIRONMENT");

        env ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (env == null) return false;

        if (env.StartsWith("dev", StringComparison.OrdinalIgnoreCase)) return true;

        if (env.StartsWith("test", StringComparison.OrdinalIgnoreCase)) return true;

        return false;

    }

}