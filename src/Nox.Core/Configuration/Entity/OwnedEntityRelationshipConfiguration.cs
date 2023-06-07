using Nox.Core.Components;
using Nox.Core.Models.Entity;

namespace Nox.Core.Configuration;

public class OwnedEntityRelationshipConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Entity { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public RelationshipType Relationship { get; set; }

    public bool AllowNavigation { get; set; } = true;
}