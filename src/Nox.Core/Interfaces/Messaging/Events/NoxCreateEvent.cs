using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Messaging.Events;

public class NoxCreateEvent<T>: INoxEvent where T: IDynamicEntity
{
    private NoxEventTypeEnum EventType => NoxEventTypeEnum.Create;
    public T? Payload { get; set; }
}