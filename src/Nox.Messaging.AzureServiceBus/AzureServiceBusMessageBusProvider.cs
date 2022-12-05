using MassTransit;
using Nox.Core.Interfaces;

namespace Nox.Messaging.AzureServiceBus;

public class AzureServiceBusMessageBusProvider: IMessageBusProvider
{
    private readonly string _connectionString;


    public AzureServiceBusMessageBusProvider(IMessagingProvider provider)
    {
        _connectionString = provider.ConnectionString!;
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