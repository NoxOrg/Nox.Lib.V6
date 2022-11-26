using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Messaging;

namespace Samples.Cli.Commands;

public class LoaderInsertMessageConsumer : IConsumer<LoaderInsertMessage>
{
    readonly ILogger<LoaderInsertMessage> _logger;

    public LoaderInsertMessageConsumer(ILogger<LoaderInsertMessage> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<LoaderInsertMessage> context)
    {
        _logger.LogInformation("Inserted: {Text}", context.Message.Value);
        
        return Task.CompletedTask;
    }
}

public class LoaderInsertMessageConsumerDefinition : ConsumerDefinition<LoaderInsertMessageConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<LoaderInsertMessageConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


public class HeartbeatConsumer : IConsumer<HeartbeatMessage>
{
    readonly ILogger<HeartbeatConsumer> _logger;

    public HeartbeatConsumer(ILogger<HeartbeatConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HeartbeatMessage> context)
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