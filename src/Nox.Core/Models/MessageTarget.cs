using Nox.Core.Components;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Core.Models;

public sealed class MessageTarget: MetaBase, IMessageTarget
{
    public string MessagingProvider { get; set; } = string.Empty;
}