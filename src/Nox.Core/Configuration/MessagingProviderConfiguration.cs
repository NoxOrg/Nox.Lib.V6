using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class MessagingProviderConfiguration: MetaBase
{
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
    public bool IsHeartbeat { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}