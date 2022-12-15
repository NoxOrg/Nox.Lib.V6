using System.Linq;
using Nox.Core.Configuration;
using Nox.Core.Validation.Configuration;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class EmptyConfigValidationTests
{
    [Test]
    public void Must_validate_empty_config()
    {
        var config = new NoxConfiguration();
        var validator = new NoxConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(3));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Name must be specified in ")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Database definition must be specified in ")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The service definition '' must have at least one entity in ")));
    }

    [Test]
    public void Must_validate_empty_database_config()
    {
        var config = new DatabaseConfiguration();
        var validator = new DatabaseConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The database/datasource name must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The database/datasource '' provider must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_messaging_provider_config()
    {
        var config = new MessagingProviderConfiguration();
        var validator = new MessagingProviderConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The messaging provider name must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The provider for messaging provider '' must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_api_config()
    {
        var config = new ApiConfiguration();
        var validator = new ApiConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The api's name must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The api must have at least one route defined in")));
    }
    
    [Test]
    public void Must_validate_empty_api_route_config()
    {
        var config = new ApiRouteConfiguration();
        var validator = new ApiRouteConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(3));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The api route name must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The api route http verb must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The api route target url must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_api_route_parameter_config()
    {
        var config = new ApiRouteParameterConfiguration();
        var validator = new ApiRouteParameterConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Api route parameter must have a name specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Api route parameter '' must have a type specified in")));
    }
    
    [Test]
    public void Must_validate_empty_api_route_response_config()
    {
        var config = new ApiRouteResponseConfiguration();
        var validator = new ApiRouteResponseConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Api route response must have a type specified in")));
    }
    
    
    [Test]
    public void Must_validate_empty_loader_config()
    {
        var config = new LoaderConfiguration();
        var validator = new LoaderConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(5));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The loader name must be specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Loader Schedule must have a Start specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Loader load strategy must have a type specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Please only use: DropAndLoad/MergeNew for Load strategy Type in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The loader target entity must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_loader_schedule_config()
    {
        var config = new LoaderScheduleConfiguration();
        var validator = new LoaderScheduleConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Loader Schedule must have a Start specified in")));
    }
    
    [Test]
    public void Must_validate_empty_loader_load_strategy_config()
    {
        var config = new LoaderLoadStrategyConfiguration();
        var validator = new LoaderLoadStrategyConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Loader load strategy must have a type specified in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("Please only use: DropAndLoad/MergeNew for Load strategy Type in")));
    }
    
    [Test]
    public void Must_validate_empty_loader_target_config()
    {
        var config = new LoaderTargetConfiguration();
        var validator = new LoaderTargetConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The loader target entity must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_loader_message_target_config()
    {
        var config = new LoaderMessageTargetConfiguration();
        var validator = new LoaderMessageTargetConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("The messaging provider must be specified in")));
    }
    
    [Test]
    public void Must_validate_empty_loader_source_config()
    {
        var config = new LoaderSourceConfiguration();
        var validator = new LoaderSourceConfigValidator();
        var result = validator.Validate(config);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Loader source is missing a DataSource value in")));
        Assert.That(result.Errors.Any(e => e.ErrorMessage.StartsWith("a Loader source is missing a Query statement in")));
    }
}