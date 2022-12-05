using Nox.Core.Interfaces;

namespace Nox.TestFixtures.Entities;

public class Person: IDynamicEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}