using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlTransformConfiguration: MetaBase
{
    public List<EtlMapConfiguration>? Map { get; set; } = new();
    public List<EtlLookupConfiguration>? Lookups { get; set; } = new();
}