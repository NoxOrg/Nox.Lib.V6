using Nox.Core.Interfaces.Database;
using Nox.Messaging.Enumerations;

namespace Nox.Messaging.Events;

public class NoxUpdateEvent<T>: INoxEvent where T: IDynamicEntity
{
    private NoxEventTypeEnum EventType => NoxEventTypeEnum.Update;
    public T? Payload { get; set; }
}