
using Microsoft.Extensions.Configuration;
using Nox.Dynamic;

internal class Program
{
    private static IConfiguration Configuration { get; set; } = null!;

    public static async Task<int> Main(string[] args)
    {

        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (IsDevelopment())
        {
            builder.AddUserSecrets<Program>();
        }

        Configuration = builder.Build();

        var dynamicService = new DynamicService.Builder()
            .WithConfiguration(Configuration)
            .FromRootFolder(Configuration["DefinitionRootPath"])
            .Build();

        dynamicService.ValidateDatabaseSchema();

        dynamicService.ExecuteDataLoaders();

        await Task.Delay(1);

        return 0;

    }

    private static bool IsDevelopment() =>
        (Environment.GetEnvironmentVariable("ENVIRONMENT")?.StartsWith("dev", StringComparison.OrdinalIgnoreCase) ?? false);

}




