using MassTransit;
using Nox;
using Nox.Core.Enumerations;

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
        _logger.LogInformation("Received CurrencyCreatedDomainEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}