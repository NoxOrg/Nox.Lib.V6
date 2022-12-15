using System.Collections.Generic;
using System.Linq;
using Nox.Core.Configuration;
using Nox.Core.Validation.Configuration;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ConfigRelationalTests
{
    [Test]
    public void If_Entity_is_used_in_loader_it_must_exist_in_entities()
    {
        var config = new NoxConfiguration
        {
            Loaders = new List<LoaderConfiguration>
            {
                new()
                {
                    Name = "TestLoader",
                    Target = new LoaderTargetConfiguration
                    {
                        Entity = "Test"
                    }
                }
            }
        };
        var validator = new NoxConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(3));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Name must be specified in ")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Database definition must be specified in ")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The service definition '' must have at least one entity in ")));
    }
}