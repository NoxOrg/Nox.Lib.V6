using MassTransit;
using Nox.Events;

namespace Samples.Api.Consumers;

public class CurrencyCreatedEventConsumer: IConsumer<CurrencyCreatedEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;

    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        _logger.LogInformation("Received CurrencyCreatedEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}