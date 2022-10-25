using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Services;

namespace Nox.Dynamic.MessageBus;

public class HeartbeatWorker : BackgroundService
{
    readonly IBus _bus;
    private readonly IDynamicService _dynamicService;

    public HeartbeatWorker(IBus bus, IDynamicService dynamicService)
    {
        _bus = bus;
        
        _dynamicService = dynamicService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new HeartbeatMessage { 
                Value = $"Heartbeat.{_dynamicService.Name}: The time is {DateTimeOffset.Now}" 
            }, stoppingToken);
    
            await Task.Delay(10000, stoppingToken);
        }
    }
}

public class HeartbeatMessage
{
    public string Value { get; set; } = string.Empty;
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
