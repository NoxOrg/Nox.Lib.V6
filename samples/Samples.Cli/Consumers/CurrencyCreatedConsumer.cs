using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Events;

namespace Samples.Cli.Consumers;

public class CurrencyCreatedEventConsumer : IConsumer<CurrencyCreatedEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;

    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CurrencyCreatedEvent> context)
    {
        _logger.LogInformation("Currency Created by {source}: {@payload}", context.Message.EventSource, context.Message.Payload);
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