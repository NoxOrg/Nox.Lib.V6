using MassTransit;

namespace Nox.Dynamic.DatabaseProviders
{
    public interface IMessageBusProvider
    {
        IBusRegistrationConfigurator ConfigureMassTransit(IBusRegistrationConfigurator configuration);
    }
}