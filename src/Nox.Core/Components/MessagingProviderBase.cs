using Nox.Core.Interfaces;

namespace Nox.Core.Components;

public class MessagingProviderBase : MetaBase, IMessagingProvider
{
    public string Name { get; set; } = string.Empty;
    public bool IsHeartbeat { get; set; } = false;
    public string Provider { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}

