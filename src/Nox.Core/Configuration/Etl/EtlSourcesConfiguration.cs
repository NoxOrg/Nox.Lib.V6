using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlSourcesConfiguration: MetaBase
{
    public List<EtlSourceMessageQueueConfiguration>? MessageQueues { get; set; } = new();
    public List<EtlSourceFileConfiguration>? Files { get; set; } = new();
    public List<EtlSourceDatabaseConfiguration>? Databases { get; set; } = new();
    public List<EtlSourceHttpConfiguration> Http { get; set; } = new();
}