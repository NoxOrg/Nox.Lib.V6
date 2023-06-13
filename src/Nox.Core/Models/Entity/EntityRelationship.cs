using Nox.Core.Components;

namespace Nox.Core.Models.Entity;

public class EntityRelationship : MetaBase, IRelationship
{
    public string Name { get; set; } = string.Empty;

    public string Entity { get; set; } = string.Empty;

    public RelationshipType Relationship { get; set; }

    public bool AllowNavigation { get; set; } = false;
}