using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlSourcesConfiguration: MetaBase
{
    public List<EtlMessageQueueConfiguration>? MessageQueues { get; set; } = new();
    public List<EtlFileConfiguration>? Files { get; set; } = new();
    public List<EtlSourceDatabaseConfiguration>? Databases { get; set; } = new();
    public List<EtlHttpConfiguration> Http { get; set; } = new();
}