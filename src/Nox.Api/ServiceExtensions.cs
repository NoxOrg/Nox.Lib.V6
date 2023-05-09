using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api.OData;

namespace Nox.Api;

public static class ServiceExtensions
{
    public static IServiceCollection AddDynamicApi(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["ServiceApiEndpointProvider"];
        switch (provider)
        {
            default:
                services.AddDynamicODataFeature();
                break;
        }

        return services;
    }
}