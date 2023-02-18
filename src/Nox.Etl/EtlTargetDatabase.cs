using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlTargetDatabase: MetaBase, IEtlTargetDatabase
{
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string StoredProc { get; set; } = string.Empty;
}