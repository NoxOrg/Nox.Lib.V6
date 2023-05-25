using Nox.Core.Components;

namespace Nox.Core.Models.Entity;

public interface IRelationship 
{
    string Name { get; set; }

    string Entity { get; set; }

    bool IsMany { get; set; }

    bool IsRequired { get; set; }
}