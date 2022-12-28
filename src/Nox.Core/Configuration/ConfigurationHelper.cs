using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Nox.Core.Constants;
using Nox.Core.Exceptions;

namespace Nox.Core.Configuration;

public class ConfigurationHelper
{
    public static IConfiguration? GetNoxAppSettings()
    {
        return GetNoxAppSettings(Array.Empty<string>());
    }

    public static IConfiguration? GetNoxAppSettings(string[] args)
    {
        var configBuilder = GetApplicationConfigurationBuilder(args);
        var config = configBuilder.Build();

        if (string.IsNullOrEmpty(config["Nox:DefinitionRootPath"]))
        {
            config["Nox:DefinitionRootPath"] = "./";
        }

        config = configBuilder.Build();
        var keyVaultUri = config["Nox:KeyVaultUri"];

        if (string.IsNullOrEmpty(keyVaultUri))
        {
            keyVaultUri = KeyVault.DefaultKeyVaultUri;
        }

        var keys = new[] {
            "AZURE_TENANT_ID",
            "AZURE_CLIENT_ID",
            "AZURE_CLIENT_SECRET",
            "AZURE_DEVOPS_PAT"
        };

        try
        {
            var secrets = GetSecrets(keyVaultUri, keys).GetAwaiter().GetResult();

            //Api endpoint config
            secrets!.Add(new KeyValuePair<string, string>("ServiceApiEndpointProvider", ""));

            configBuilder.AddInMemoryCollection(secrets!);

        }
        catch
        {
            // throw new ConfigurationException($"Error loading secrets from vault at '{keyVaultUri}'");
        }

        return configBuilder.Build();
    }

    private static async Task<IList<KeyValuePair<string,string>>?> GetSecrets(string keyVaultUri, string[] keys)
    {
        var secrets = new List<KeyValuePair<string,string>>();

        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        try
        {
            foreach (var key in keys)
            {
                var secret = await keyVault.GetSecretAsync(keyVaultUri, key.Replace(":", "--").Replace("_","-"));
                secrets.Add(new KeyValuePair<string, string>(key, secret.Value ?? ""));
            }
        }
        catch (Exception ex) 
        {
            throw new ConfigurationException($"Error loading secrets from vault at '{keyVaultUri}'. ({ex.Message})");
        }

        return secrets;
    }

    private static IConfigurationBuilder GetApplicationConfigurationBuilder(string[] args)
    {
        var env = GetEnvironment();
        
        var pathToContentRoot = Directory.GetCurrentDirectory();

        //AppSettings
        var json = Path.Combine(pathToContentRoot, "appsettings.json");

        //AppSettings.Env
        var envJson = "";
        if (!string.IsNullOrEmpty(env))
        {
            envJson = Path.Combine(pathToContentRoot, $"appsettings.{env}.json");
        }
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(pathToContentRoot)
            .AddJsonFile(json, true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        if (File.Exists(json)) builder.AddJsonFile(json);

        if (!string.IsNullOrEmpty(envJson) && File.Exists(envJson)) builder.AddJsonFile(envJson);

        if (IsDevelopment())
        {
            builder.AddUserSecrets<ConfigurationHelper>();
        }

        return builder;

    }

    public static bool IsDevelopment()
    {
        var env = GetEnvironment();

        if (env == null) return false;

        if (env.StartsWith("dev", StringComparison.OrdinalIgnoreCase)) return true;

        if (env.StartsWith("test", StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    private static string? GetEnvironment()
    {
        var env = Environment.GetEnvironmentVariable("ENVIRONMENT");
        env ??= Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        env ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env;
    }

}