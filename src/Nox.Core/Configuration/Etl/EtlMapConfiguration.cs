using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlMapConfiguration: MetaBase
{
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetAttribute { get; set; } = string.Empty;
    public string Converter { get; set; } = string.Empty;
}