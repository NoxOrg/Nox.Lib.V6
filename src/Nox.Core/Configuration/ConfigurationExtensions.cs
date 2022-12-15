using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Validation.Configuration;

namespace Nox.Core.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddNoxConfiguration(this IServiceCollection services, string designRoot)
    {
        if (!Directory.Exists(designRoot)) designRoot = "./";
        var configurator = new NoxConfigurator(designRoot);
        var config = configurator.LoadConfiguration();
        var validator = new NoxConfigValidator();
        validator.ValidateAndThrow(config);
        services.AddSingleton<INoxConfiguration>(config);
        return services;
    }
}