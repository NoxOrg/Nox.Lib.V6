using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlMap: IEtlMap
{
    public string SourceColumn { get; set; }
    public string TargetAttribute { get; set; }
    public string Converter { get; set; }
}