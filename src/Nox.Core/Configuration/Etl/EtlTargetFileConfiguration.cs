using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlTargetFileConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}