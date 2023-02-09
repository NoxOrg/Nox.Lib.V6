using Nox.Core.Components;
using Nox.Core.Interfaces.Configuration;

namespace Nox.Core.Configuration;

public class EtlTargetDatabaseConfiguration: MetaBase, IEtlTargetConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string StoredProc { get; set; } = string.Empty;
}