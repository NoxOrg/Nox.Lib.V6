using Nox.Core.Components;
using Nox.Core.Interfaces.Configuration;

namespace Nox.Core.Configuration;

public class EtlConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public List<EtlSourcesConfiguration>? Sources { get; set; } = new();
    public List<EtlTargetsConfiguration>? Targets { get; set; } = new();
}