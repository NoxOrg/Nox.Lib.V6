using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Nox.Dynamic.MetaData;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class AzureServiceBusMessageBusProvider : IMessageBusProvider
    {

        private string _connectionString;


        public AzureServiceBusMessageBusProvider(IServiceMessageBus serviceBus)
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
            configuration.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(_connectionString);

                cfg.ConfigureEndpoints(context);
            });
            return configuration;
        }
    }
}
