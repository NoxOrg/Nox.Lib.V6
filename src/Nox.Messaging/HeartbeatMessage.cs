using Nox.Core.Interfaces;

namespace Nox.Messaging;

public class HeartbeatMessage: IHeartbeatMessage
{
    public string Value { get; set; } = string.Empty;
}