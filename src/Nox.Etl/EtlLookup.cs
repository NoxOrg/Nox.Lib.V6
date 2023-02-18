using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlLookup: IEtlLookup
{
    public string SourceColumn { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string MatchColumn { get; set; } = string.Empty;
    public string ReturnColumn { get; set; } = string.Empty;
    public string TargetAttribute { get; set; } = string.Empty;
}