using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlMap: IEtlMap
{
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetAttribute { get; set; } = string.Empty;
    public string Converter { get; set; } = string.Empty;
}