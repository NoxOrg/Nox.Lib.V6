using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public static class ServiceExtension
{
    public static IServiceCollection AddLoaderExecutor(this IServiceCollection services)
    {
        services.AddSingleton<IEtlExecutor, EtlExecutor>();
        return services;
    }
}