using EntityAttribute = Nox.Core.Models.EntityAttribute;

namespace Nox.Core.Interfaces;

public interface IEntity : IMetaBase
{
    ICollection<EntityAttribute> Attributes { get; set; }
    string Description { get; set; }
    string Name { get; set; }
    string PluralName { get; set; }
    List<string> RelatedChildren { get; set; }
    string RelatedChildrenJson { get; set; }
    List<string> RelatedParents { get; set; }
    string RelatedParentsJson { get; set; }
    string Schema { get; set; }
    int SortOrder { get; set; }
    string Table { get; set; }
    bool ApplyDefaults();
}
