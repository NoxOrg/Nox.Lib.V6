using Microsoft.Extensions.Hosting;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;
using Nox.Messaging;

namespace Nox.Lib;

public class HeartbeatWorker : BackgroundService
{
    private readonly IDynamicService _dynamicService;
    private readonly INoxMessenger? _messenger;

    public HeartbeatWorker(
        IDynamicService dynamicService, 
        INoxMessenger? messenger)
    {
        _dynamicService = dynamicService;
        _messenger = messenger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _messenger?.SendHeartbeat(new HeartbeatMessage { Value = $"Heartbeat.{_dynamicService.Name}: The time is {DateTimeOffset.Now}" }, stoppingToken);
            await Task.Delay(10000, stoppingToken);
        }
    }
}



