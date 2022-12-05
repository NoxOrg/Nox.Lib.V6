namespace Nox.Core.Interfaces;

public interface INoxMessenger
{
    Task SendMessage(ILoader loader, object message);
    Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken);
}