using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nox.Core.Extensions;
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
        var parentYaml = await File.ReadAllTextAsync("./yaml-files/parent.yaml");
        parentYaml.ResolveYamlReferences();
        var builder = new DeserializerBuilder();
        var deserializer = builder
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var parent = deserializer.Deserialize<YamlParent>(parentYaml);

    }
}