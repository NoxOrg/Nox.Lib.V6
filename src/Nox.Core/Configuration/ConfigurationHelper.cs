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
        return configBuilder.Build();
    }

    private static IConfigurationBuilder GetApplicationConfigurationBuilder(string[] args)
    {
        var env = GetEnvironment();
        
        var pathToContentRoot = Directory.GetCurrentDirectory();

        //AppSettings
        var json = Path.Combine(pathToContentRoot, "appsettings.json");

        //AppSettings.Env
        var envJson = string.IsNullOrEmpty(env) ? "" : Path.Combine(pathToContentRoot, $"appsettings.{env}.json");

        var builder = new ConfigurationBuilder()
            .SetBasePath(pathToContentRoot)
            .AddJsonFile(json, true, true);
        
        if (!string.IsNullOrEmpty(envJson) && File.Exists(envJson)) builder.AddJsonFile(envJson);
        
        if (IsDevelopment())
        {
            builder.AddUserSecrets<ConfigurationHelper>();
        }

        builder.AddEnvironmentVariables();
        
        builder.AddCommandLine(args);
        
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

    public static async Task<IList<KeyValuePair<string, string>>?> GetNoxSecrets(IConfiguration config, string[] keys)
    {
        var keyVaultUri = config["Nox:KeyVaultUri"];

        if (string.IsNullOrEmpty(keyVaultUri))
        {
            keyVaultUri = KeyVault.DefaultKeyVaultUri;
        }

        return await GetSecretsFromVault(keyVaultUri, keys);
    }


    private static async Task<IList<KeyValuePair<string, string>>?> GetSecretsFromVault(string keyVaultUri, string[] keys)
    {
        var secrets = new List<KeyValuePair<string, string>>();

        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        try
        {
            foreach (var key in keys)
            {
                var secret = await keyVault.GetSecretAsync(keyVaultUri, key.Replace(":", "--").Replace("_", "-"));
                secrets.Add(new KeyValuePair<string, string>(key, secret.Value ?? ""));
            }
        }
        catch (Exception ex)
        {
            throw new ConfigurationException($"Error loading secrets from vault at '{keyVaultUri}'. ({ex.Message})");
        }

        return secrets;
    }



}