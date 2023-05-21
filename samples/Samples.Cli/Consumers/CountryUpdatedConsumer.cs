using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Events;

namespace Samples.Cli.Consumers;

public class CountryUpdatedEventConsumer : IConsumer<CountryUpdatedEvent>
{
    readonly ILogger<CountryUpdatedEventConsumer> _logger;

    public CountryUpdatedEventConsumer(ILogger<CountryUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CountryUpdatedEvent> context)
    {
        _logger.LogInformation("Country Updated by {source}: {@payload}", context.Message.EventSource, context.Message.Payload);
        return Task.CompletedTask;
    }
}

public class CountryUpdatedEventConsumerDefinition : ConsumerDefinition<CountryUpdatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CountryUpdatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}