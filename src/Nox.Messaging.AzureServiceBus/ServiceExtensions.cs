using MassTransit;

namespace Nox.Messaging.AzureServiceBus;

public static class ServiceExtensions
{
    public static void UseAzureServiceBus(this IBusRegistrationConfigurator configurator, string connectionString)
    {
        configurator.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(connectionString);
            cfg.ConfigureEndpoints(context);
        });
    }
}