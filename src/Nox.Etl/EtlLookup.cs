using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlLookup: IEtlLookup
{
    public string SourceColumn { get; set; }
    public string DataSource { get; set; }
    public string MatchColumn { get; set; }
    public string ReturnColumn { get; set; }
    public string TargetAttribute { get; set; }
}