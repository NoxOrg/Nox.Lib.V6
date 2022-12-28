using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Interfaces.Messaging;

public interface INoxMessenger
{
    Task SendMessage(ILoader loader, object message);
    Task SendMessage(IEntity entity, object message);
    Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken);
}