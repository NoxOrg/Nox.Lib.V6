using System.Reflection;
using System.Reflection.Emit;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Database;

public interface IDynamicDbEntity
{
    string Name { get; }
    string PluralName { get; }
    TypeBuilder TypeBuilder { get; }
    IEntity Entity { get; }
    Type Type { get; }
    MethodInfo DbContextGetCollectionMethod { get; }
    MethodInfo DbContextGetSingleResultMethod { get; }
    MethodInfo DbContextGetObjectPropertyMethod { get; }
    MethodInfo DbContextGetNavigationMethod { get; }
    MethodInfo DbContextPostMethod { get; }
    MethodInfo DbContextPutMethod { get; }
    MethodInfo DbContextPatchMethod { get; }
    MethodInfo DbContextDeleteMethod { get; }
}
