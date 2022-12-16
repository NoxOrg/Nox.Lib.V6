using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Lib;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Messaging;

namespace Nox;

public static class ServiceExtensions
{
    private static readonly IConfiguration? _configuration = ConfigurationHelper.GetNoxAppSettings();

    public static IServiceCollection AddNox(
        this IServiceCollection services)
    {
        if (_configuration == null)
        {
            throw new ConfigurationException("Could not load Nox configuration.");
        }

        services
            .AddNoxConfiguration(_configuration["Nox:DefinitionRootPath"]!)
            .AddDataProviderFactory()
            .AddDynamicApi(_configuration)
            .AddData()
            .AddEtl()
            .AddMicroservice()
            .AddNoxMessaging(false)
            .AddJobScheduler();
            
        return services;
    }

    private static IServiceCollection AddMicroservice(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicService, DynamicService>();
        services.AddHostedService<HeartbeatWorker>();
        return services;
    }
}