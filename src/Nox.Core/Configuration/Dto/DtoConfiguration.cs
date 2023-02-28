using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class DtoConfiguration: MetaBase
{
    public List<DtoAttributeConfiguration> Attributes { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
}