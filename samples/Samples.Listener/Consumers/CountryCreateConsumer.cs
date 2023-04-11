using Microsoft.Extensions.Logging;

namespace Samples.Listener.Consumers;

public class CountryCreateConsumer: CountryCreatedEventConsumer
{
    public Task Consume(ConsumeContext<CountryCreatedDomainEvent> context)
    {
        _logger.LogInformation("Country Created by {source}: {@payload}", context.Message.EventSource, context.Message.Payload);
        //Developer code here
        return Task.CompletedTask;
    }
}