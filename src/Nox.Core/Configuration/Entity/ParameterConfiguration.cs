using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ParameterConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = "int";

    public string DefaultValue { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = true;
}