using MassTransit;
using Microsoft.Extensions.Logging;

namespace FO.Consumer;

public class INT001_CustomerConsumer : IConsumer<INT001_Customer>
{
    readonly ILogger<INT001_Customer> _logger;

    public INT001_CustomerConsumer(ILogger<INT001_Customer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<INT001_Customer> context)
    {

        // inititiate message header
        // hydrate the message body
        // push to esb
        // log the status

        _logger.LogInformation("INT001_Customer Type: {Text}", context.Message.Type);
        _logger.LogInformation("INT001_Customer Message: {Text}", context.Message.Value);

        return Task.CompletedTask;
    }
}

public class INT001_CustomerConsumerDefinition : ConsumerDefinition<INT001_CustomerConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<INT001_CustomerConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


