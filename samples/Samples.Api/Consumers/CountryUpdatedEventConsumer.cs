using MassTransit;
using Nox;
using Nox.Core.Enumerations;

namespace Samples.Api.Consumers;

public class CountryUpdatedEventConsumer: IConsumer<CountryUpdatedDomainEvent>
{
    readonly ILogger<CountryUpdatedEventConsumer> _logger;

    public CountryUpdatedEventConsumer(ILogger<CountryUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CountryUpdatedDomainEvent> context)
    {
        _logger.LogInformation($"Received CountryUpdatedDomainEvent from {context.Message.EventSource.ToFriendlyName()}: {context.Message.Payload}");
        return Task.CompletedTask;
    }
}