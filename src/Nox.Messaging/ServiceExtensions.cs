using System.Reflection;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Extensions;
using Nox.Core.Helpers;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Messaging.Events;
using Nox.Messaging.AmazonSQS;
using Nox.Messaging.AzureServiceBus;
using Nox.Messaging.RabbitMQ;

namespace Nox.Messaging;

public static class ServiceExtensions
{

    public static IServiceCollection AddNoxListeners(this IServiceCollection services)
    {
        services.AddNoxMessaging(true);

        return services;
    }

    public static IServiceCollection AddNoxMessaging(
        this IServiceCollection services,
        bool isExternalListener = true)
    {
        if (isExternalListener)
        {
            var appSettings = ConfigurationHelper.GetNoxAppSettings();
            services.AddNoxConfiguration((appSettings != null ? appSettings["Nox:DefinitionRootPath"] : "")!);
        }

        if (services == null) throw new ArgumentNullException(nameof(services));
        var svcProvider = services.BuildServiceProvider();
        var noxConfig = svcProvider.GetRequiredService<IProjectConfiguration>();

        services.AddSingleton<INoxMessenger, NoxMessenger>();

        //Create the messaging providers if not defined in yaml
        noxConfig.MessagingProviders ??= new List<MessagingProviderConfiguration>();

        //Ensure Mediator is added
        if (noxConfig.MessagingProviders.All(mp => !mp.Provider!.ToLower().Equals("mediator")))
        {
            noxConfig.MessagingProviders.Add(new MessagingProviderConfiguration() { Provider = "Mediator", Name = "Mediator" });
        }

        var isRabbitAdded = false;
        var isAzureAdded = false;
        var isAmazonAdded = false;
        var isMemoryAdded = false;

        foreach (var msgProvider in noxConfig.MessagingProviders)
        {
            switch (msgProvider.Provider!.ToLower())
            {
                case "rabbitmq":
                    if (!isRabbitAdded)
                    {
                        services.AddRabbitMqBus(msgProvider, isExternalListener);
                        isRabbitAdded = true;
                    }

                    break;
                case "azureservicebus":
                    if (!isAzureAdded)
                    {
                        services.AddAzureBus(msgProvider, isExternalListener);
                        isAzureAdded = true;
                    }

                    break;
                case "amazonsqs":
                    if (!isAmazonAdded)
                    {
                        services.AddAmazonBus(msgProvider, isExternalListener);
                        isAmazonAdded = true;
                    }

                    break;
                case "inmemory":
                    if (!isMemoryAdded)
                    {
                        services.AddInMemoryBus();
                        isMemoryAdded = true;
                    }

                    break;

                case "mediator":
                    if (!isExternalListener) services.AddNoxMediator();
                    break;
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
        services.AddMassTransit<IAzureBus>(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();

            //todo By default, sagas are in-memory, but should be changed to a durable saga repository.
            mt.SetInMemorySagaRepositoryProvider();
            
            if (isExternalListener)
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
        services.AddMassTransit<IAmazonBus>(mt =>
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

            // TODO: there should be no secrets in the yaml. We should maybe look at supporting special ${ENV_VARIABLE_NAME} expressions for 
            // connection strings and other environment variable/secret injection. Have opened an Issue to resolve.
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