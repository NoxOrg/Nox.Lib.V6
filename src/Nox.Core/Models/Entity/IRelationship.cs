namespace Nox.Core.Models.Entity;

public interface IRelationship 
{
    string Name { get; set; }

    string Entity { get; set; }

    RelationshipType Relationship { get; set; }

    bool AllowNavigation { get; set; }
}