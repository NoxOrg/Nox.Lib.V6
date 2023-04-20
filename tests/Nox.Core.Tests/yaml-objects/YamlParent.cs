using System.Collections.Generic;

namespace Nox.Core.Tests.yaml_objects;

public class YamlParent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<YamlChild> AttributeOne { get; set; } = new();
    public List<YamlChild> AttributeTwo { get; set; } = new();
    public string Closure { get; set; } = string.Empty;
}