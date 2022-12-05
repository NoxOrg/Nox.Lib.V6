using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Exceptions;
using Nox.Core.Interfaces;

namespace Nox.Core.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddNoxConfiguration(this IServiceCollection services, string designRoot)
    {
        if (!Directory.Exists(designRoot)) designRoot = "./";
        var configurator = new NoxConfigurator(designRoot);
        var config = configurator.LoadConfiguration();
        services.AddSingleton<INoxConfiguration>(config);
        return services;
    }
}