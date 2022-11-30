using MassTransit;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Messaging.AzureServiceBus;

public class AzureServiceBusMessageBusProvider: IMessageBusProvider
{
    private readonly string _connectionString;


    public AzureServiceBusMessageBusProvider(IServiceMessageBus serviceBus)
    {
        _connectionString = serviceBus.ConnectionString!;
    }

    public IBusRegistrationConfigurator ConfigureMassTransit(IBusRegistrationConfigurator configuration)
    {
        configuration.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(_connectionString);

            cfg.ConfigureEndpoints(context);
        });
        return configuration;
    }
}