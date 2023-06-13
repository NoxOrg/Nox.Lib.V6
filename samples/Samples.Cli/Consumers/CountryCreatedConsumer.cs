using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Events;

namespace Samples.Cli.Consumers;

public class CountryCreatedEventConsumer : IConsumer<CountryCreatedEvent>
{
    readonly ILogger<CountryCreatedEventConsumer> _logger;

    public CountryCreatedEventConsumer(ILogger<CountryCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CountryCreatedEvent> context)
    {
        _logger.LogInformation("Country Created by {source}: {@payload}", context.Message.EventSource, context.Message.Payload);
        //Developer code here
        return Task.CompletedTask;
    }
}

public class CountryCreatedEventConsumerDefinition : ConsumerDefinition<CountryCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CountryCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


