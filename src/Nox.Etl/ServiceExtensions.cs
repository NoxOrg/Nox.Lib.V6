using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

namespace Nox.Etl;

public static class ServiceExtensions
{
    public static IServiceCollection AddEtl(this IServiceCollection services)
    {
        services.AddSingleton<IEtlExecutor, EtlExecutor>();
        return services;
    }
}