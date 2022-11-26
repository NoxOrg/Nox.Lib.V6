using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Nox.Mediation;

public static class ServiceExtension
{
    public static IServiceCollection AddMediation(this IServiceCollection services)
    {
        services.AddMediator();
        return services;
    }
}