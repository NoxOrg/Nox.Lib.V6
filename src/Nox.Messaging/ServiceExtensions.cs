using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Messaging;
using Nox.Messaging.AmazonSQS;
using Nox.Messaging.AzureServiceBus;
using Nox.Messaging.Events;
using Nox.Messaging.RabbitMQ;

namespace Nox.Messaging;

public static class ServiceExtensions
{
    public static IServiceCollection AddNoxMessaging(
        this IServiceCollection services,
        bool isExternalListener = true)
    {
        if (isExternalListener)
        {
            var appSettings = ConfigurationHelper.GetNoxAppSettings();
            services.AddNoxConfiguration((appSettings != null ? appSettings["Nox:DefinitionRootPath"] : "")!);
        }

        if (services == null) throw new ArgumentNullException(nameof (services));
        var svcProvider = services.BuildServiceProvider();
        var noxConfig = svcProvider.GetRequiredService<INoxConfiguration>();
        
        if (noxConfig.MessagingProviders != null && noxConfig.MessagingProviders.Any())
        {
            services.AddSingleton<INoxMessenger, NoxMessenger>();
            foreach (var msgProvider in noxConfig.MessagingProviders)
            {
                switch (msgProvider.Provider!.ToLower())
                {
                    case "rabbitmq":
                        services.AddRabbitMqBus(msgProvider, isExternalListener); 
                        break;
                    case "azureservicebus":
                        services.AddAzureBus(msgProvider, isExternalListener); 
                        break;
                    case "amazonsqs":
                        services.AddAmazonBus(msgProvider, isExternalListener); 
                        break;
                    case "inmemory": 
                        services.AddInMemoryBus();
                        break;
                    case "mediator":
                        services.AddNoxMediator();
                        break;
                }
            }
        }

        services.AddNoxEvents();
        return services;
    }

    public static IServiceCollection AddNoxEvents(this IServiceCollection services, Assembly? assembly = null)
    {
        if (assembly == null) assembly = Assembly.GetEntryAssembly();
        foreach (var implementationType in assembly!.GetTypes().Where(m => !m.IsInterface && m.GetInterfaces().Any(i => i == typeof(INoxEvent))))
        {
            services.AddSingleton(typeof(INoxEvent), implementationType);
        }

        return services;
    }

    private static void AddNoxMediator(this IServiceCollection services)
    {
        services.AddMediator( mt =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            mt.AddConsumers(entryAssembly);
            mt.AddSagaStateMachines(entryAssembly);
            mt.AddSagas(entryAssembly);
            mt.AddActivities(entryAssembly);
        });
    }

    private static void AddRabbitMqBus(this IServiceCollection services, MessagingProviderConfiguration config, bool isExternalListener)
    {
        services.AddMassTransit<IRabbitMqBus>(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();

            //todo By default, sagas are in-memory, but should be changed to a durable saga repository.
            mt.SetInMemorySagaRepositoryProvider();
            
            if(isExternalListener)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                mt.AddConsumers(entryAssembly);
                mt.AddSagaStateMachines(entryAssembly);
                mt.AddSagas(entryAssembly);
                mt.AddActivities(entryAssembly);
            }
            mt.UseRabbitMq(config.ConnectionString!);
        });
    }
    
    private static void AddAzureBus(this IServiceCollection services, MessagingProviderConfiguration config, bool isExternalListener)
    {
        services.AddMassTransit<IRabbitMqBus>(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();

            //todo By default, sagas are in-memory, but should be changed to a durable saga repository.
            mt.SetInMemorySagaRepositoryProvider();
            
            if(isExternalListener)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                mt.AddConsumers(entryAssembly);
                mt.AddSagaStateMachines(entryAssembly);
                mt.AddSagas(entryAssembly);
                mt.AddActivities(entryAssembly);
            }

            mt.UseAzureServiceBus(config.ConnectionString!);
        });
    }
    
    private static void AddAmazonBus(this IServiceCollection services, MessagingProviderConfiguration config, bool isExternalListener)
    {
        services.AddMassTransit<IRabbitMqBus>(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();

            //todo By default, sagas are in-memory, but should be changed to a durable saga repository.
            mt.SetInMemorySagaRepositoryProvider();
            
            if(isExternalListener)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                mt.AddConsumers(entryAssembly);
                mt.AddSagaStateMachines(entryAssembly);
                mt.AddSagas(entryAssembly);
                mt.AddActivities(entryAssembly);
            }

            mt.UseAmazonSqs(config.ConnectionString!, config.AccessKey!, config.SecretKey!);
        });
    }
    
    private static void AddInMemoryBus(this IServiceCollection services)
    {
        services.AddMassTransit<IRabbitMqBus>(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();
            mt.SetInMemorySagaRepositoryProvider();
            var entryAssembly = Assembly.GetEntryAssembly();
            mt.AddConsumers(entryAssembly);
            mt.AddSagaStateMachines(entryAssembly);
            mt.AddSagas(entryAssembly);
            mt.AddActivities(entryAssembly);
            mt.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}