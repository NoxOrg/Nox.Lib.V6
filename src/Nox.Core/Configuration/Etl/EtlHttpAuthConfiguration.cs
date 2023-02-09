using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlHttpAuthConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}