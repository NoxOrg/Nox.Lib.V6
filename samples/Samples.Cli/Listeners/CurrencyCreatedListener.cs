using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox;

namespace Samples.Cli.Listeners;

public class CurrencyCreatedEventConsumer : IConsumer<CurrencyCreatedDomainEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;

    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CurrencyCreatedDomainEvent> context)
    {
        _logger.LogInformation("Currency Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class CurrencyCreatedEventConsumerDefinition : ConsumerDefinition<CurrencyCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CurrencyCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


