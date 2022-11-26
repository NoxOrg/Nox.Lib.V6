using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Nox.Mediator;


public static class ServiceExtension
{
    public static IServiceCollection AddDynamicMediator(this IServiceCollection services)
    {
        services.AddMediator();
        return services;
    }
}