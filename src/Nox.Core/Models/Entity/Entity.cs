using Humanizer;
using Nox.Core.Components;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Core.Models.Entity;

public sealed class Entity : MetaBase, IEntity
{
    public EntityKey Key { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public string Table { get; set; } = null!;
    public string Schema { get; set; } = "dbo";

    public ICollection<EntityQuery> Queries { get; set; } = new Collection<EntityQuery>();

    public ICollection<EntityCommand> Commands { get; set; } = new Collection<EntityCommand>();

    public ICollection<EntityRelationship> Relationships { get; set; } = new Collection<EntityRelationship>();

    public ICollection<OwnedEntityRelationship> OwnedRelationships { get; set; } = new Collection<OwnedEntityRelationship>();

    public ICollection<IRelationship> AllRelationships => Relationships.Cast<IRelationship>()
            .Union(OwnedRelationships)
            .Union(Key.IsComposite
                    ? Key.Entities.Select(key => new EntityRelationship
                    {
                        Entity = key,
                        Name = key,
                        AllowNavigation = true,
                        Relationship = RelationshipType.ExactlyOne
                    }).AsEnumerable()
                    : Enumerable.Empty<IRelationship>()) // Include composite key if exists
            .ToList();

    [NotMapped]
    public ICollection<string> RelatedParents => AllRelationships
            .Where(r => r.Relationship == RelationshipType.ExactlyOne || r.Relationship == RelationshipType.ZeroOrOne)
            .Select(r => r.Entity)
            .ToList();

    public int SortOrder { get; set; }
    public ICollection<EntityAttribute> Attributes { get; set; } = new Collection<EntityAttribute>();

    ICollection<IMessageTarget>? IEntity.Messaging
    {
        get => Messaging?.ToList<IMessageTarget>();
        set => Messaging = value as ICollection<MessageTarget>;
    }

    public ICollection<MessageTarget>? Messaging { get; set; }

    public bool ApplyDefaults()
    {
        if (string.IsNullOrWhiteSpace(PluralName))
            PluralName = Name.Pluralize();

        if (string.IsNullOrWhiteSpace(Table))
            Table = Name;

        if (string.IsNullOrWhiteSpace(Schema))
            Schema = "dbo";

        return true;
    }
}