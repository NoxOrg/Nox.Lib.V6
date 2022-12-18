using MassTransit;
using Nox;

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
        _logger.LogInformation("Received CountryUpdatedDomainEvent: {Text}", context.Message.Payload);
        return Task.CompletedTask;
    }
}