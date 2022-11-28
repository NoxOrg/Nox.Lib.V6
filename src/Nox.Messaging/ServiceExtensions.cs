using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Messaging.AmazonSQS;
using Nox.Messaging.AzureServiceBus;
using Nox.Messaging.Events;
using Nox.Messaging.RabbitMQ;

namespace Nox.Messaging;

public static class ServiceExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services,
        IConfiguration config, bool isServer = true)
    {
        var provider = config["ServiceMessageBusProvider"];
        var connectionString = config["ServiceMessageBusConnectionString"];
        var connectionVariable = config["ServiceMessageBusConnectionVariable"];
        var accessKey = config["ServiceMessageBusAccessKeyVariable"];
        var secretKey = config["ServiceMessageBusSecretKeyVariable"];

        if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(connectionVariable))
        {
            connectionString = config[connectionVariable];
        }

        services.AddMassTransit(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();

            // By default, sagas are in-memory, but should be changed to a durable
            // saga repository.
            mt.SetInMemorySagaRepositoryProvider();

            if (isServer)
            {
                var noxAssembly = Assembly.GetExecutingAssembly();
                mt.AddConsumers(noxAssembly);
                mt.AddSagaStateMachines(noxAssembly);
                mt.AddSagas(noxAssembly);
                mt.AddActivities(noxAssembly);
            }
            else
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                mt.AddConsumers(entryAssembly);
                mt.AddSagaStateMachines(entryAssembly);
                mt.AddSagas(entryAssembly);
                mt.AddActivities(entryAssembly);
            }

            switch (provider)
            {
                case "rabbitmq":
                    mt.UseRabbitMq(connectionString);
                    break;

                case "azureservicebus":
                    mt.UseAzureServiceBus(connectionString);
                    break;
                case "amazonsqs":
                    mt.UseAmazonSqs(connectionString, accessKey, secretKey);
                    break;
                case "inmemory":
                    mt.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                    break;
            }
        });

        return services;
    }

    public static void AddNoxEvents(this IServiceCollection services, Assembly? assembly = null)
    {
        if (assembly == null) assembly = Assembly.GetExecutingAssembly();
        foreach (var implementationType in assembly.GetTypes().Where(m => !m.IsInterface && m.GetInterfaces().Any(i => i == typeof(INoxEvent))))
        {
            services.AddSingleton(typeof(INoxEvent), implementationType);
        }
    }
}