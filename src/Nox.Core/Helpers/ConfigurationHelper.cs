using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Nox.Core.Constants;
using Nox.Core.Exceptions;
using Nox.Core.Interfaces.Configuration;
using Nox.Utilities.Secrets;

namespace Nox.Core.Helpers;

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

    public static async Task<IList<KeyValuePair<string, string>>?> GetNoxSecrets(IProjectConfiguration projectConfig, IPersistedSecretStore secretCache, string[] keys)
    {
        var resolvedSecrets = new List<KeyValuePair<string, string>>();
        if (projectConfig.Secrets != null)
        {
            var ttl = new TimeSpan(0, 30, 0);
            var validFor = projectConfig.Secrets.ValidFor;
            if (validFor != null)
            {
                ttl = new TimeSpan(validFor.Days ?? 0, validFor.Hours ?? 0, validFor.Minutes ?? 0, validFor.Seconds ?? 0);

            }
            if (ttl == TimeSpan.Zero) ttl = new TimeSpan(0, 30, 0);
            //Resolve the secret from the cache first
            foreach (var key in keys)
            {
                var cachedSecret = await secretCache.LoadAsync($"{projectConfig.Name}.{key}", ttl);
                resolvedSecrets.Add(new KeyValuePair<string, string>(key, cachedSecret ?? ""));
            }
            
            //resolve any remaining secrets from the vaults
            var unresolvedSecrets = resolvedSecrets.Where(s => s.Value == "").ToList();
            if (unresolvedSecrets.Any() && projectConfig.Secrets.Providers != null)
            {
                foreach (var vault in projectConfig.Secrets.Providers)
                {
                    if (!unresolvedSecrets.Any()) break;
                    switch (vault.Provider.ToLower())
                    {
                        case "azure-keyvault":
                            var azureVault = new AzureSecretProvider(vault.Url);
                            var azureSecrets = azureVault.GetSecretsAsync(unresolvedSecrets.Select(k => k.Key).ToArray()).Result;
                            if (azureSecrets != null)
                            {
                                if (azureSecrets.Any()) resolvedSecrets.AddRange(azureSecrets);
                                foreach (var azureSecret in azureSecrets)
                                {
                                    await secretCache.SaveAsync($"{projectConfig.Name}.{azureSecret.Key}", azureSecret.Value);
                                }
                            }
                            break;
                    }
                    unresolvedSecrets = resolvedSecrets.Where(s => s.Value == "").ToList();
                }
            }
        }

        return resolvedSecrets;
    }
}