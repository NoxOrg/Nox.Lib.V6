using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
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
            new NoxSolutionBuilder()
                .UseDependencyInjection(services)
                .Build();
        }

        if (services == null) throw new ArgumentNullException(nameof(services));
        var svcProvider = services.BuildServiceProvider();
        var solution = svcProvider.GetRequiredService<NoxSolution>();

        services.AddSingleton<INoxMessenger, NoxMessenger>();

        var isRabbitAdded = false;
        var isAzureAdded = false;
        var isAmazonAdded = false;
        var isMemoryAdded = false;
        
        //Always add mediator 
        services.AddNoxMediator();
        
        if (solution.Infrastructure != null)
        {
            if (solution.Infrastructure.Messaging?.IntegrationEventServer != null)
            {
                var ieServer = solution.Infrastructure.Messaging.IntegrationEventServer;
                switch (ieServer.Provider)
                {
                    case MessagingServerProvider.RabbitMq:
                        services.AddRabbitMqBus(ieServer, isExternalListener);
                        isRabbitAdded = true;
                        break;
                    case MessagingServerProvider.AzureServiceBus:
                        services.AddAzureBus(ieServer, isExternalListener);
                        isAzureAdded = true;
                        break;
                    case MessagingServerProvider.AmazonSqs:
                        services.AddAmazonBus(ieServer, isExternalListener);
                        isAmazonAdded = true;
                        break;
                    case MessagingServerProvider.InMemory:
                        services.AddInMemoryBus();
                        isMemoryAdded = true;
                        break;
                }
            }

            if (solution.Infrastructure.Dependencies?.DataConnections != null)
            {
                foreach (var dataConnection in solution.Infrastructure.Dependencies?.DataConnections!)
                {
                    switch (dataConnection.Provider)
                    {
                        case DataConnectionProvider.RabbitMq:
                            if (!isRabbitAdded)
                            {
                                services.AddRabbitMqBus(dataConnection, isExternalListener);
                                isRabbitAdded = true;
                            }
                            break;
                        case DataConnectionProvider.AzureServiceBus:
                            if (!isAzureAdded)
                            {
                                services.AddAzureBus(dataConnection, isExternalListener);
                                isAzureAdded = true;
                            }
                            break;
                        case DataConnectionProvider.AmazonSqs:
                            if (!isAmazonAdded)
                            {
                                services.AddAmazonBus(dataConnection, isExternalListener);
                                isAmazonAdded = true;
                            }
                            break;
                        case DataConnectionProvider.InMemory:
                            if (!isMemoryAdded)
                            {
                                services.AddInMemoryBus();
                                isMemoryAdded = true;
                            }

                            break;
                    }
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

    private static void AddRabbitMqBus(this IServiceCollection services, ServerBase msgServer, bool isExternalListener)
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

            //todo build a connection string parser that can take ServerBase and return a connection string                  
            mt.UseRabbitMq(msgServer.ServerUri!);
        });
    }
    
    private static void AddAzureBus(this IServiceCollection services, ServerBase msgServer, bool isExternalListener)
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

            mt.UseAzureServiceBus(msgServer.ServerUri!);
        });
    }
    
    private static void AddAmazonBus(this IServiceCollection services, ServerBase msgServer, bool isExternalListener)
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

            mt.UseAmazonSqs(msgServer.ServerUri!, msgServer.User!, msgServer.Password!);
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