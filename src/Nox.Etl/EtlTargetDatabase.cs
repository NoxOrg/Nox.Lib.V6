using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlTargetDatabase: MetaBase, IEtlTargetDatabase
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string Provider { get; set; }
    public string StoredProc { get; set; }
}