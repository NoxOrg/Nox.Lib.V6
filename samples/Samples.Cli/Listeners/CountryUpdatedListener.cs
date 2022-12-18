using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox;

namespace Samples.Cli.Listeners;

public class CountryUpdatedEventConsumer : IConsumer<CountryUpdatedDomainEvent>
{
    readonly ILogger<CountryUpdatedEventConsumer> _logger;

    public CountryUpdatedEventConsumer(ILogger<CountryUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CountryUpdatedDomainEvent> context)
    {
        _logger.LogInformation("Country Updated: {Text}", JsonSerializer.Serialize(context.Message.Payload));
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


