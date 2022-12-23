using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxDeleteEvent<T>: INoxEvent where T: IDynamicEntity
{
    private NoxEventTypeEnum EventType => NoxEventTypeEnum.Delete;
    public T? Payload { get; set; }
}