using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Messaging.Enumerations;

namespace Nox.Messaging.Events;

public class NoxCreateEvent<T>: INoxEvent where T: IDynamicEntity
{
    private NoxEventTypeEnum EventType => NoxEventTypeEnum.Create;
    public T? Payload { get; set; }
}