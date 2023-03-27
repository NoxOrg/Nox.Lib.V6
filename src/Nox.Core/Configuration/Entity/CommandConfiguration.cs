using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class CommandConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public List<string> Events { get; set; } = new();

    public List<ParameterConfiguration> Parameters { get; set; } = new();
}