using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using YamlDotNet.Serialization;
using ETLBoxOffice.LicenseManager;
using Nox.Configuration.Models;

namespace Nox.Configuration;

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

        if (config["Nox:DefinitionRootPath"] == null)
        {
            throw new ConfigurationException("Could not find 'Nox:DefinitionRootPath' in environment or appsettings.json");
        }
        
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        var path = Path.GetFullPath(config["Nox:DefinitionRootPath"]);

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

        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusProvider", service.MessageBus.Provider.ToLower()));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionString", service.MessageBus.ConnectionString ?? ""));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionVariable", service.MessageBus.ConnectionVariable ?? ""));

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

        return builder;

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