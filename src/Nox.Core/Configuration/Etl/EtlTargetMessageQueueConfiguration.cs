using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlTargetMessageQueueConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public EtlRetryPolicyConfiguration? Retry { get; set; } = new();
}