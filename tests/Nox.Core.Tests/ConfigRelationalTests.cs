using System.Collections.Generic;
using System.Linq;
using Nox.Core.Configuration;
using Nox.Core.Validation.Configuration;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ConfigRelationalTests
{
    [Test]
    public void If_Entity_is_used_in_loader_it_must_exist_in_Entities()
    {
        var loaderConfig = new LoaderTargetConfiguration
        {
            DefinitionFileName = "test.yaml",
            Entity = "TestEntity"
        };
        var entitiesConfig = new List<EntityConfiguration>();
        
        var validator = new LoaderTargetConfigValidator(entitiesConfig);
        var result = validator.Validate(loaderConfig);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The loader targets a non existing entity")));
        entitiesConfig.Add(new EntityConfiguration
        {
            Name = "TestEntity"
        });
        result = validator.Validate(loaderConfig);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public void If_DataSource_is_used_in_loader_it_must_exist_in_DataSources()
    {
        var sourceConfig = new LoaderSourceConfiguration
        {
            DefinitionFileName = "test.yaml",
            DataSource = "TestSource"
        };
        var sourcesConfig = new List<DataSourceConfiguration>();
        
        var validator = new LoaderSourceConfigValidator(sourcesConfig);
        var result = validator.Validate(sourceConfig);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The Loader data source TestSource in test.yaml does not exist in the service definition yaml")));
        sourcesConfig.Add(new DataSourceConfiguration
        {
            Name = "TestSource"
        });
        result = validator.Validate(sourceConfig);
        Assert.That(!result.Errors.Any(e => e.ErrorMessage.StartsWith("The Loader data source TestSource in test.yaml does not exist in the service definition yaml")));
    }
    
    [Test]
    public void If_MessageTarget_is_used_in_loader_it_must_exist_in_MessagingProviders()
    {
        var targetConfig = new MessageTargetConfiguration()
        {
            DefinitionFileName = "test.yaml",
            MessagingProvider = "TestProvider"
        };
        var providersConfig = new List<MessagingProviderConfiguration>();
        
        var validator = new MessageTargetConfigValidator(providersConfig);
        var result = validator.Validate(targetConfig);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The Loader messaging target TestProvider in test.yaml does not exist in the service definition yaml")));
        providersConfig.Add(new MessagingProviderConfiguration
        {
            Name = "TestProvider"
        });
        result = validator.Validate(targetConfig);
        Assert.That(!result.Errors.Any(e => e.ErrorMessage.StartsWith("The Loader messaging target TestProvider in test.yaml does not exist in the service definition yaml")));
    }
}