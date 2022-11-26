using MassTransit;
using Microsoft.Extensions.Hosting;
using Nox.Core.Interfaces;
using Nox.Messaging;

namespace Nox.Microservice;

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



