using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api.OData;
using Nox.Solution;

namespace Nox.Api;

public static class ServiceExtensions
{
    public static IServiceCollection AddDynamicApi(this IServiceCollection services, NoxSolution solution)
    {
        if (solution.Infrastructure is { Endpoints: not null })
        {
            switch (solution.Infrastructure.Endpoints.ApiServer)
            {
                default: //oData
                    services.AddDynamicODataFeature();
                    services.AddDynamicQueriesAndCommands();
                    break;
            } 
        }
        
        return services;
    }
}