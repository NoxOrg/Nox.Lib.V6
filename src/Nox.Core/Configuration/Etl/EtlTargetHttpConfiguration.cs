using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlTargetHttpConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Verb { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public EtlHttpAuthConfiguration? Auth { get; set; } = new();
}