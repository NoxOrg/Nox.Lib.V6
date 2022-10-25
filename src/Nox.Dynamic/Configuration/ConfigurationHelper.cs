using MassTransit;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Nox.Dynamic.Constants;
using Nox.Dynamic.MetaData;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace Nox.Dynamic.Configuration;

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
        
        var deserializer = new DeserializerBuilder().Build();

        var path = Path.GetFullPath(config["Nox:DefinitionRootPath"]);

        var service = Directory
            .EnumerateFiles(path, DefinitionFilePattern.SERVICE_DEFINITION_PATTERN, SearchOption.AllDirectories)
            .Take(1)
            .Select(f =>
            {
                var svc = deserializer.Deserialize<Service>(File.ReadAllText(f));
                svc.DefinitionFileName = Path.GetFullPath(f);
                svc.Database.DefinitionFileName = Path.GetFullPath(f);
                return svc;
            })
            .FirstOrDefault();

        if (service == null)
        {
            throw new ConfigurationException($"Could not find file matching '{DefinitionFilePattern.SERVICE_DEFINITION_PATTERN}' in '{path}'");
        }

        var keys = new[] {
            "ConnectionString--AzureServiceBus",
            "ConnectionString--MasterDataSource",
            "XECurrency--ApiPassword",
            "XECurrency--ApiUser",
            "EtlBox--LicenseKey",
        };

        var secrets = GetSecrets(service.KeyVaultUri, keys);
        
        if (secrets == null)
        {
            throw new ConfigurationException($"Error loading secrets from vault at '{service.KeyVaultUri}'");
        }

        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusProvider", service.MessageBus.Provider.ToLower()));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionString", service.MessageBus.ConnectionString ?? ""));
        secrets.Add(new KeyValuePair<string, string>("ServiceMessageBusConnectionVariable", service.MessageBus.ConnectionVariable ?? ""));

        configBuilder.AddInMemoryCollection(secrets);

        return configBuilder.Build();
    }

    private static IList<KeyValuePair<string,string>>? GetSecrets(string keyVaultUri, string[] keys)
    {
        var secrets = new List<KeyValuePair<string,string>>();

        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        try
        {
            foreach (var key in keys)
            {
                var value = keyVault.GetSecretAsync(keyVaultUri, key).GetAwaiter().GetResult().Value;
                secrets.Add(new KeyValuePair<string, string>(key.Replace("--",":"), value ?? ""));
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