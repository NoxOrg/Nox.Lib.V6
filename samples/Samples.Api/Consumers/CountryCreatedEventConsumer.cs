using MassTransit;
using Nox;

namespace Samples.Api.Consumers;

public class CountryCreatedEventConsumer: IConsumer<CountryCreatedDomainEvent>
{
    public async Task Consume(ConsumeContext<CountryCreatedDomainEvent> context)
    {
        throw new NotImplementedException();
    }
}