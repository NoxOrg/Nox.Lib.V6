using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxCreatedEvent<T>: INoxEvent where T: IDynamicEntity
{
    public NoxEventType EventType => NoxEventType.Created;
    public NoxEventSource EventSource { get; set; }
    public T? Payload { get; set; }
}