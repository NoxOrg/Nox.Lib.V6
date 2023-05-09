using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlLookupConfiguration: MetaBase
{
    public string SourceColumn { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string MatchColumn { get; set; } = string.Empty;
    public string ReturnColumn { get; set; } = string.Empty;
    public string TargetAttribute { get; set; } = string.Empty;
}