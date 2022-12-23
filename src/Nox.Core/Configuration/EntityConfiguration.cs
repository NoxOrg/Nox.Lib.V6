using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityConfiguration: MetaBase
{
    public List<EntityAttributeConfiguration> Attributes { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public List<string>? RelatedParents { get; set; }
    public string Schema { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public string Table { get; set; } = string.Empty;
    public List<MessageTargetConfiguration>? Messaging { get; set; } = new();
}