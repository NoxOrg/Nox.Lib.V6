using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox;
using Nox.Core.Enumerations;

namespace Samples.Cli.Consumers;

public class CurrencyUpdatedEventConsumer : IConsumer<CurrencyUpdatedDomainEvent>
{
    readonly ILogger<CurrencyUpdatedEventConsumer> _logger;

    public CurrencyUpdatedEventConsumer(ILogger<CurrencyUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CurrencyUpdatedDomainEvent> context)
    {
        _logger.LogInformation("Currency Updated by {source}: {@payload}", context.Message.EventSource, context.Message.Payload);
        return Task.CompletedTask;
    }
}

public class CurrencyUpdatedEventConsumerDefinition : ConsumerDefinition<CurrencyUpdatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CurrencyUpdatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


