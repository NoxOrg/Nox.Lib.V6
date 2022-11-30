using Nox.Core.Interfaces;

namespace Nox.Core.Components;

public class ServiceMessageBusBase : MetaBase, IServiceMessageBus
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }

    // [NotMapped]
    // public IMessageBusProvider? MessageBusProvider { get; set; }
}

