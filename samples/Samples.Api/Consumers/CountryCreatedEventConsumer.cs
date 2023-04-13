using MassTransit;
using Nox.Events;
using Nox.Data;

namespace Samples.Api.Consumers;

public class CountryCreatedEventConsumer: IConsumer<CountryCreatedEvent>
{
    readonly ILogger<CountryCreatedEventConsumer> _logger;

    public CountryCreatedEventConsumer(ILogger<CountryCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CountryCreatedEvent> context)
    {
        _logger.LogInformation("Received CountryCreatedDomainEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}