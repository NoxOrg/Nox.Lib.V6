using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces.Messaging;

namespace Samples.Cli.Listeners;

public class HeartbeatConsumer : IConsumer<IHeartbeatMessage>
{
    readonly ILogger<HeartbeatConsumer> _logger;

    public HeartbeatConsumer(ILogger<HeartbeatConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<IHeartbeatMessage> context)
    {
        _logger.LogInformation("Received Heartbeat: {Text}", context.Message.Value);
        return Task.CompletedTask;
    }
}

public class HeartbeatConsumerDefinition : ConsumerDefinition<HeartbeatConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<HeartbeatConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}
