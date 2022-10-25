using MassTransit;
using Microsoft.Extensions.Configuration;
using Nox.Dynamic.MetaData;
using System.Configuration;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class RabbitMqMessageBusProvider : IMessageBusProvider
    {

        private readonly string _connectionString;

        public RabbitMqMessageBusProvider(IServiceMessageBus serviceBus)
        {
            if (serviceBus != null)
            {
                _connectionString = serviceBus.ConnectionString!;
            }
            else
            {
                _connectionString = "";
            }
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
}
