using MassTransit;
using Nox.Events;

namespace Samples.Api.Consumers;

public class CountryUpdatedEventConsumer: IConsumer<CountryUpdatedEvent>
{
    readonly ILogger<CountryUpdatedEventConsumer> _logger;

    public CountryUpdatedEventConsumer(ILogger<CountryUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CountryUpdatedEvent> context)
    {
        _logger.LogInformation("Received CountryUpdatedEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}