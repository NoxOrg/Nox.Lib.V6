using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Interfaces.Messaging;

public interface INoxMessenger
{
    Task SendMessage(ILoader loader, object message);
    Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken);
}