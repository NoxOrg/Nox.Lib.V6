using MassTransit;
using Nox.Core.Interfaces;

namespace Nox.Messaging.RabbitMQ;

public class RabbitMqMessageBusProvider: IMessageBusProvider
{
    private readonly string _connectionString;

    public RabbitMqMessageBusProvider(IMessagingProvider provider)
    {
        _connectionString = provider.ConnectionString!;
    }

    public IBusRegistrationConfigurator ConfigureMassTransit(IBusRegistrationConfigurator configuration)
    {
        configuration.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(_connectionString));

            cfg.ConfigureEndpoints(context);
        });
        return configuration;
    }
}