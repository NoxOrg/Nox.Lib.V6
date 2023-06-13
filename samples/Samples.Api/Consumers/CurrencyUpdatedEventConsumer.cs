using MassTransit;
using Nox.Events;

namespace Samples.Api.Consumers;

public class CurrencyUpdatedEventConsumer: IConsumer<CurrencyUpdatedEvent>
{
    readonly ILogger<CurrencyUpdatedEventConsumer> _logger;

    public CurrencyUpdatedEventConsumer(ILogger<CurrencyUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyUpdatedEvent> context)
    {
        _logger.LogInformation("Received CurrencyUpdatedEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}