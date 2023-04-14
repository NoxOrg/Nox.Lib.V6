using System.Collections.Generic;

namespace Nox.Core.Tests.yaml_objects;

public class YamlParent
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string?> Attributes { get; set; }
    public string Closure { get; set; }
}