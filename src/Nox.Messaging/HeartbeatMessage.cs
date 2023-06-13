using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Messaging;

public class HeartbeatMessage: IHeartbeatMessage
{
    public string Value { get; set; } = string.Empty;
}