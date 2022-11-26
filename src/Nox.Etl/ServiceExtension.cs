using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

namespace Nox.Etl;

public static class ServiceExtension
{
    public static IServiceCollection AddLoaderExecutor(this IServiceCollection services)
    {
        services.AddSingleton<ILoaderExecutor, LoaderExecutor>();
        return services;
    }
}