using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxDomainEvent<T> : INoxEvent 
    where T : IDynamicDTO
{
    public NoxEventType EventType => NoxEventType.Domain;
    public NoxEventSource EventSource => NoxEventSource.Command;
    public T? Payload { get; init; }
}