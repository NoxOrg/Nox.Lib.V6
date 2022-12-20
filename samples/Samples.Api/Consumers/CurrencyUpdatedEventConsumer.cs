using MassTransit;
using Nox;

namespace Samples.Api.Consumers;

public class CurrencyUpdatedEventConsumer: IConsumer<CurrencyUpdatedDomainEvent>
{
    readonly ILogger<CurrencyUpdatedEventConsumer> _logger;

    public CurrencyUpdatedEventConsumer(ILogger<CurrencyUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyUpdatedDomainEvent> context)
    {
        _logger.LogInformation("Received CurrencyUpdatedDomainEvent: {Text}", context.Message.Payload);
        return Task.CompletedTask;
    }
}