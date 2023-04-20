using Nox.Core.Interfaces.Messaging;
using Nox.Core.Models;
using EntityAttribute = Nox.Core.Models.EntityAttribute;

namespace Nox.Core.Interfaces.Entity;

public interface IEntity : IMetaBase
{
    EntityKey Key { get; }
    string Name { get; set; }
    string Description { get; set; }
    ICollection<string> RelatedParents { get; }
    ICollection<EntityAttribute> Attributes { get; set; }
    ICollection<IMessageTarget>? Messaging { get; set; }
    ICollection<EntityRelationship> Relationships { get; set; }
    ICollection<EntityRelationship> OwnedRelationships { get; set; }
    string PluralName { get; set; }
    string Schema { get; set; }
    int SortOrder { get; set; }
    string Table { get; set; }
    bool ApplyDefaults();
}
