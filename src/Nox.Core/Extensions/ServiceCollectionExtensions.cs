using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Builders;
using Nox.Core.Interfaces;

namespace Nox.Core.Extensions;

public static class ServiceCollectionExtensions
{

    private static IProjectConfiguration? _projectConfig;

    public static IProjectConfiguration? NoxProjectConfiguration => _projectConfig;

    public static IServiceCollection AddNoxConfiguration(this IServiceCollection services, string? designRoot = null)
    {
        _projectConfig = new ProjectConfigurationBuilder(designRoot)
            .Build();
        if (_projectConfig != null)
        {
            services.AddSingleton<IProjectConfiguration>(_projectConfig);    
        }
        
        return services;
    }

    public static bool ConfirmNoxConfigurationAdded(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(IProjectConfiguration));
    }
}