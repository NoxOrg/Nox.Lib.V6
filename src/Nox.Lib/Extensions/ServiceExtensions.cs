using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Api.OData.Swagger;
using Nox.Core.Extensions;
using Nox.Core.Helpers;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Lib;
using Nox.Messaging;
using Nox.Utilities.Secrets;

namespace Nox;

public static class ServiceExtensions
{
    private static readonly IConfiguration? _configuration = ConfigurationHelper.GetNoxAppSettings();

    public static IServiceCollection AddNox(
        this IServiceCollection services,
        NoxSwaggerConfiguration? swaggerConfiguration = null)
    {
        var designRoot = "./";
        if (_configuration?["Nox:DefinitionRootPath"] != null)
        {
            designRoot = _configuration["Nox:DefinitionRootPath"];
        }

        if (services.AddNoxConfiguration(designRoot!).ConfirmNoxConfigurationAdded())
        {
            services
                .AddPersistedSecretStore()
                .AddNoxMessaging(false)
                .AddDataProviderFactory()
                .AddDynamicApi(_configuration!)
                .AddData()
                .AddEtl()
                .AddMicroservice()
                .AddJobScheduler();
        }

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