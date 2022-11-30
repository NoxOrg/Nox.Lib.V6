using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Api;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Etl;
using Nox.Jobs;
using Nox.Messaging;

namespace Nox.Microservice.Extensions;

public static class ServiceExtensions
{
    private static readonly IConfiguration? _configuration = ConfigurationHelper.GetNoxConfiguration();

    public static IServiceCollection AddNox(this IServiceCollection services)
    {
        if (_configuration == null)
        {
            throw new ConfigurationException("Could not load Nox configuration.");
        }

        services
            .AddDatabaseProviderFactory()
            .AddDynamicApi(_configuration)
            .AddDynamicModel()
            .AddDynamicDbContext()
            .AddLoaderExecutor()
            .AddDynamicService()
            .AddMessageBus(_configuration)
            .AddJobSchedulerFeature()
            .AddHeartbeat();

        return services;
    }

    public static IServiceCollection AddDynamicService(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicService, DynamicService>();
        return services;
    }
    private static IServiceCollection AddHeartbeat(this IServiceCollection services)
    {
        services.AddHostedService<HeartbeatWorker>();
        return services;
    }
}