using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EventConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Dto { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;
}