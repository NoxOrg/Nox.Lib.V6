using System.Diagnostics;
using ETLBoxOffice.LicenseManager;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Exceptions;
using YamlDotNet.Serialization;

namespace Nox.Core.Configuration;

public class ConfigurationHelper
{

    public static IConfiguration? GetNoxConfiguration()
    {
        return GetNoxConfiguration(Array.Empty<string>());
    }

    public static IConfiguration? GetNoxConfiguration(string[] args)
    {

        var configBuilder = GetApplicationConfigurationBuilder(args);

        var config = configBuilder.Build();

        if (string.IsNullOrEmpty(config["Nox:DefinitionRootPath"]))
        {
            throw new ConfigurationException("Could not find 'Nox:DefinitionRootPath' in environment or appsettings");
        }
        
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        var path = Path.GetFullPath(config["Nox:DefinitionRootPath"]);
        if (Directory.GetFiles(path, "*.yaml").Length == 0) throw new ConfigurationException($"Could not find any yaml files in {path}");
        if (Directory.GetDirectories(path).Length == 0) throw new ConfigurationException($"Could not find any entity folders in {path}");

        var service = Directory
            .EnumerateFiles(path, FileExtension.ServiceDefinition, SearchOption.AllDirectories)
            .Take(1)
            .Select(f =>
            {
                var svc = deserializer.Deserialize<ServiceBase>(File.ReadAllText(f));
                return svc;
            })
            .FirstOrDefault();

        if (service == null)
        {
            throw new ConfigurationException($"Could not find file matching '{FileExtension.ServiceDefinition}' in '{path}'");
        }

        var keys = new[] {
            "ConnectionString:AzureServiceBus",
            "ConnectionString:MasterDataSource",
            "XECurrency:ApiPassword",
            "XECurrency:ApiUser",
            "EtlBox:LicenseKey",
        };

        var secrets = GetSecrets(service.KeyVaultUri, keys).GetAwaiter().GetResult();
        
        if (secrets == null)
        {
            throw new ConfigurationException($"Error loading secrets from vault at '{service.KeyVaultUri}'");
        }

        //Message bus config
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusProvider", service.MessageBus.Provider.ToLower()));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionString", service.MessageBus.ConnectionString ?? ""));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionVariable", service.MessageBus.ConnectionVariable ?? ""));
        
        //Api endpoint config
        secrets.Add(new KeyValuePair<string, string>("ServiceApiEndpointProvider", service.EndpointProvider.ToLower()));

        configBuilder.AddInMemoryCollection(secrets);

        LicenseCheck.LicenseKey = secrets.First(s => s.Key.Equals("EtlBox:LicenseKey")).Value;

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
                var secret = await keyVault.GetSecretAsync(keyVaultUri, key.Replace(":", "--"));
                secrets.Add(new KeyValuePair<string, string>(key, secret.Value ?? ""));
            }
        }
        catch (Exception ex) 
        {
            throw new ConfigurationException($"Error loading secrets from vault at '{keyVaultUri}'. ({ex.Message})");
        }

        return secrets;
    }

    public static IConfiguration GetApplicationConfiguration(string[] args)
    {
        return GetApplicationConfigurationBuilder(args).Build();
    }

    private static IConfigurationBuilder GetApplicationConfigurationBuilder(string[] args)
    {
        var env = GetEnvironment();
        var pathToContentRoot = Directory.GetCurrentDirectory();
        var json = Path.Combine(pathToContentRoot, "appsettings.json");
        if (!string.IsNullOrEmpty(env))
        {
            var envJson = Path.Combine(pathToContentRoot, $"appsettings.{env}.json");
            if (File.Exists(envJson)) json = envJson;
        }

        if (!File.Exists(json))
        {
            var pathToExe = Process.GetCurrentProcess()?.MainModule?.FileName;
            pathToContentRoot = Path.GetDirectoryName(pathToExe);
        }
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(pathToContentRoot)
            .AddJsonFile(json, true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

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