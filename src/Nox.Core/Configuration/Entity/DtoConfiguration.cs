using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class DtoConfiguration : MetaBase
{    
    public List<EntityAttributeConfiguration> Attributes { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<EntityRelationshipConfiguration>? Relationships { get; set; } = new();
}