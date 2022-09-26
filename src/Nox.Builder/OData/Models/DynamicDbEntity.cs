using Nox.Dynamic.Dto;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Nox.OData.Models
{
    public class DynamicDbEntity
    {
        public string Name { get; init; } = null!;
        public string PluralName { get; init; } = null!;
        public TypeBuilder TypeBuilder { get; init; } = null!;
        public Entity Entity { get; init; } = null!;
        public Type Type { get; init; } = null!;
        public MethodInfo DbContextGetCollectionMethod { get; init; } = null!;
        public MethodInfo DbContextGetSingleResultMethod { get; init; } = null!;
        public MethodInfo DbContextGetObjectPropertyMethod { get; init; } = null!;
        public MethodInfo DbContextGetNavigationMethod { get; init; } = null!;

    }

}
