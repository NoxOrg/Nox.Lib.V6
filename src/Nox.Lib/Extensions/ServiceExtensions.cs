using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Lib;
using Nox.Core.Configuration;
using Nox.Core.Extensions;
using Nox.Core.Helpers;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Messaging;
using Nox.Utilities.Secrets;

namespace Nox;

public static class ServiceExtensions
{
    private static readonly IConfiguration? Configuration = ConfigurationHelper.GetNoxAppSettings();

    public static IServiceCollection AddNox(
        this IServiceCollection services)
    {
        var designRoot = "./";
        if (Configuration?["Nox:DefinitionRootPath"] != null)
        {
            designRoot = Configuration["Nox:DefinitionRootPath"];
        }

        if (services.AddNoxConfiguration(designRoot!))
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

    private static IServiceCollection AddMicroservice(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicService, DynamicService>();
        services.AddHostedService<HeartbeatWorker>();
        return services;
    }
}