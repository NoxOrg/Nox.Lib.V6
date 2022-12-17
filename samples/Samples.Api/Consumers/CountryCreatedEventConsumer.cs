using MassTransit;
using Nox;

namespace Samples.Api.Consumers;

public class CurrencyCreatedEventConsumer: IConsumer<CurrencyCreatedDomainEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;

    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyCreatedDomainEvent> context)
    {
        _logger.LogInformation("Received CurrencyCreatedDomainEvent: {Text}", context.Message.Payload);
        return Task.CompletedTask;
    }
}