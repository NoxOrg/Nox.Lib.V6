using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Api.OData.Swagger;
using Nox.Core.Extensions;
using Nox.Core.Helpers;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Lib;
using Nox.Messaging;
using Nox.Model;
using Nox.Utilities.Secrets;

namespace Nox;

public static class ServiceExtensions
{
    private static readonly IConfiguration? Configuration = ConfigurationHelper.GetNoxAppSettings();

    public static IServiceCollection AddNox(
        this IServiceCollection services,
        NoxSwaggerConfiguration? swaggerConfiguration = null)
    {
        HandleSwaggerGeneration(services, swaggerConfiguration);

        var designRoot = "./";
        if (Configuration?["Nox:DefinitionRootPath"] != null)
        {
            designRoot = Configuration["Nox:DefinitionRootPath"];
        }

        if (services.AddNoxConfiguration(designRoot!).ConfirmNoxConfigurationAdded())
        {
            services
                .AddPersistedSecretStore()
                .AddNoxMessaging(false)
                .AddDataProviderFactory()
                .AddDynamicApi(Configuration!)
                .AddData()
                .AddEtl()
                .AddMicroservice()
                .AddJobScheduler();
        }

        return services;
    }

    private static void HandleSwaggerGeneration(
        IServiceCollection services,
        NoxSwaggerConfiguration? swaggerConfiguration)
    {
        swaggerConfiguration ??= new NoxSwaggerConfiguration();

        if (!swaggerConfiguration.UseSwaggerGen)
        {
            return;
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
    }

    private static IServiceCollection AddMicroservice(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicService, DynamicService>();
        services.AddHostedService<HeartbeatWorker>();
        return services;
    }
}