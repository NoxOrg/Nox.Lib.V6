using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Nox.Dynamic.Configuration;

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

    public static bool IsDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ENVIRONMENT");

        env ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (env == null) return false;

        if (env.StartsWith("dev", StringComparison.OrdinalIgnoreCase)) return true;

        if (env.StartsWith("test", StringComparison.OrdinalIgnoreCase)) return true;

        return false;

    }

}