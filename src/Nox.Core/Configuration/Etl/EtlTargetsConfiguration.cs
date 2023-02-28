using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlTargetsConfiguration: MetaBase
{
    public List<EtlTargetMessageQueueConfiguration>? MessageQueues { get; set; } = new();
    public List<EtlTargetFileConfiguration>? Files { get; set; } = new();
    public List<EtlTargetDatabaseConfiguration>? Databases { get; set; } = new();
    public List<EtlTargetHttpConfiguration> Http { get; set; } = new();
}