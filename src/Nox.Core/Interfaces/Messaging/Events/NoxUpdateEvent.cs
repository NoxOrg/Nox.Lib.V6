using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxUpdateEvent<T>: INoxEvent where T: IDynamicEntity
{
    private NoxEventTypeEnum EventType => NoxEventTypeEnum.Update;
    public T? Payload { get; set; }
}