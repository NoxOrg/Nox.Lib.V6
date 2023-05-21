using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxUpdatedEvent<T> : INoxEvent 
    where T : IDynamicEntity
{
    public NoxEventType EventType => NoxEventType.Updated;
    public NoxEventSource EventSource { get; set; }
    public T? Payload { get; set; }
}