using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Exceptions;
using Nox.Core.Interfaces;

namespace Nox.Core.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddNoxConfiguration(this IServiceCollection services, string designRoot)
    {
        if (string.IsNullOrEmpty(designRoot)) throw new ConfigurationException("Nox 'DesignRootPath setting may not be empty! Please verify you appsettings or arguments.'");
        if (Directory.GetFiles(designRoot, "*.yaml").Length == 0) throw new ConfigurationException($"Could not find any yaml files in {designRoot}");
        if (Directory.GetDirectories(designRoot).Length == 0) throw new ConfigurationException($"Could not find any entity folders in {designRoot}");
        var configurator = new NoxConfigurator(designRoot);
        var config = configurator.LoadConfiguration();        
        services.AddSingleton<INoxConfiguration>(config);
        return services;
    }
}