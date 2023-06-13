using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public static class ServiceExtensions
{
    public static IServiceCollection AddEtl(this IServiceCollection services)
    {
        services.AddSingleton<IEtlExecutor, EtlExecutor>();
        return services;
    }
}