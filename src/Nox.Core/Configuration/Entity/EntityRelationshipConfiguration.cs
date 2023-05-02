using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityRelationshipConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Entity { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsMany { get; set; } = false;

    public bool IsOneWay { get; set; } = false;

    public bool IsRequired { get; set; } = false;
}