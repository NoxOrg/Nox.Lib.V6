using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nox.Core.Extensions;
using Nox.Core.Helpers;
using Nox.Core.Tests.yaml_objects;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nox.Core.Tests;

public class YamlTests
{
    [Test]
    public async Task Can_deserialize_yaml_with_include_tag()
    {
        var yaml = YamlHelper.ResolveYamlReferences("./yaml-files/parent.yaml");
        var builder = new DeserializerBuilder();
        var deserializer = builder
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var parent = deserializer.Deserialize<YamlParent>(yaml);
        Assert.NotNull(parent);
        Assert.That(parent.AttributeOne.Count, Is.EqualTo(2));
        Assert.That(parent.AttributeTwo.Count, Is.EqualTo(2));
        Assert.That(parent.Name, Is.EqualTo("This is the parent yaml file"));
        Assert.That(parent.Description, Is.EqualTo("This file contains a $ref tag"));
        Assert.That(parent.Closure, Is.EqualTo("This is the end of the line"));
    }
}