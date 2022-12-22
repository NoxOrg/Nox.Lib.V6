using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;

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

    public virtual bool ApplyDefaults()
    {
        var isValid = true;
        Provider = Provider.Trim().ToLower();
        return isValid;
    }

}