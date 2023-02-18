using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class MessageTargetConfiguration: MetaBase
{
    public string MessagingProvider { get; set; } = string.Empty;
}