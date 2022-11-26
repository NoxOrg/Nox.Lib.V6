using MassTransit;

namespace Nox.Messaging.RabbitMQ;

public static class ServiceExtensions
{
    public static void UseRabbitMq(this IBusRegistrationConfigurator configurator, string connectionString)
    {
        configurator.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(connectionString);
            cfg.ConfigureEndpoints(context);
        });
    }
}