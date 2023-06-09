using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Api.OData.Swagger;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Lib;
using Nox.Messaging;
using Nox.Solution;
using Nox.Utilities.Secrets;


namespace Nox;

public static class ServiceExtensions
{
    public static IServiceCollection AddNox(
        this IServiceCollection services,
        NoxSwaggerConfiguration? swaggerConfiguration = null)
    {
        var solution = new NoxSolutionBuilder()
            .UseDependencyInjection(services)
            .Build();
        services
            .AddPersistedSecretStore()
            .AddNoxMessaging(false)
            .AddDataProviderFactory()
            .AddDynamicApi(solution)
            .AddData()
            .AddEtl()
            .AddMicroservice()
            .AddJobScheduler();

        services.AddNoxSwaggerGeneration(swaggerConfiguration);

        return services;
    }

    public static IServiceCollection AddNoxSwaggerGeneration(
        this IServiceCollection services,
        NoxSwaggerConfiguration? swaggerConfiguration)
    {
        swaggerConfiguration ??= new NoxSwaggerConfiguration();

        if (!swaggerConfiguration.UseSwaggerGen)
        {
            return services;
        }
        
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(cfg =>
        {
            //Add this to ensure swagger document is correctly annotated
            cfg.EnableAnnotations();

            if (swaggerConfiguration.UseODataEntitySectionsSwaggerFilter)
            {
                cfg.DocumentFilter<ODataEntitySectionsSwaggerFilter>();
            }

            swaggerConfiguration.SetupAction(cfg);
        });

        return services;
    }

    private static IServiceCollection AddMicroservice(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicService, DynamicService>();
        services.AddHostedService<HeartbeatWorker>();
        return services;
    }
}