using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;

namespace Nox.TestFixtures.Entities;

public class Person: IDynamicEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}