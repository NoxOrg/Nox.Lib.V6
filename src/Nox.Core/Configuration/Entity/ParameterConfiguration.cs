using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ParameterConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "int";

    public bool IsRequired { get; set; } = true;
}