using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Extensions;
using Nox.Core.Interfaces;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ProjectConfigurationTests: ConfigurationTestFixture
{
    [Test]
    public void Project_config_should_be_defaulted_and_validated_when_resolved()
    {
        TestServices!.AddNoxConfiguration("./design/Project_Config");
        var provider = TestServices.BuildServiceProvider();
        var config = provider.GetRequiredService<IProjectConfiguration>();
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("Test"));
        Assert.That(config.Description, Is.EqualTo("Test Microservice"));
        Assert.That(config.DefinitionFileName, Is.Not.Empty);
        Assert.That(config.EndpointProvider, Is.EqualTo(""));
        //Api
        Assert.That(config.Apis, Is.Not.Null);
        Assert.That(config.Apis!.Count, Is.EqualTo(1));
        var apis = config.Apis.ToList();
        Assert.That(apis[0].Name, Is.EqualTo("TestApi"));
        Assert.That(apis[0].Description, Is.EqualTo("Test entity controller and endpoints"));
        Assert.That(apis[0].DefinitionFileName, Is.Not.Empty);
        Assert.That(apis[0].Routes!.Count, Is.EqualTo(3));
        var apiRoutes = apis[0].Routes!.ToList();
        Assert.That(apiRoutes[0].Name, Is.EqualTo("Persons"));
        Assert.That(apiRoutes[0].Description, Is.EqualTo("Returns all persons"));
        Assert.That(apiRoutes[0].HttpVerb, Is.EqualTo("GET"));
        Assert.That(apiRoutes[0].TargetUrl, Is.EqualTo("odata/Persons"));
        Assert.That(apiRoutes[0].Responses!.Count, Is.EqualTo(1));
        //Database
        Assert.That(config.Database, Is.Not.Null);
        Assert.That(config.Database!.Name, Is.EqualTo("Test"));
        Assert.That(config.Database!.ConnectionString, Is.EqualTo("Data Source=./DataTest.db"));
        //Entities
        Assert.That(config.Entities, Is.Not.Null);
        Assert.That(config.Entities!.Count, Is.EqualTo(2));
        //Person
        var person = config.Entities.FirstOrDefault(e => e.Name == "Person");
        Assert.That(person, Is.Not.Null);
        Assert.That(person!.Attributes.Count, Is.EqualTo(3));
        Assert.That(person.DefinitionFileName, Is.Not.Empty);
        Assert.That(person.Description, Is.EqualTo("Persons"));
        Assert.That(person.Name, Is.EqualTo("Person"));
        //This is important as this should be defaulted by configurator
        Assert.That(person.PluralName, Is.EqualTo("Persons"));
        Assert.That(person.Schema, Is.EqualTo("dbo"));
        Assert.That(person.Table, Is.EqualTo("Person"));
        
        //Vehicle
        var vehicle = config.Entities.FirstOrDefault(e => e.Name == "Vehicle");
        Assert.That(vehicle, Is.Not.Null);
        Assert.That(vehicle!.Attributes.Count, Is.EqualTo(3));
        Assert.That(vehicle.DefinitionFileName, Is.Not.Empty);
        Assert.That(vehicle.Description, Is.EqualTo("Vehicles"));
        Assert.That(vehicle.Name, Is.EqualTo("Vehicle"));
        Assert.That(vehicle.PluralName, Is.EqualTo("Vehicles"));
        Assert.That(vehicle.RelatedParents!.Count, Is.EqualTo(1));
        Assert.That(vehicle.Schema, Is.EqualTo("dbo"));
        Assert.That(vehicle.Table, Is.EqualTo("Vehicle"));

        //Loaders        
        Assert.That(config.Loaders, Is.Not.Null);
        Assert.That(config.Loaders!.Count, Is.EqualTo(2));
        var vehicleLoader = config.Loaders.FirstOrDefault(l => l.Name == "VehicleLoader");
        Assert.That(vehicleLoader, Is.Not.Null);
        Assert.That(vehicleLoader!.DefinitionFileName, Is.Not.Empty);
        Assert.That(vehicleLoader.Description, Is.EqualTo("Loads vehicle data"));
        Assert.That(vehicleLoader.LoadStrategy, Is.Not.Null);
        Assert.That(vehicleLoader.LoadStrategy!.Columns, Is.Not.Null);
        Assert.That(vehicleLoader.LoadStrategy!.Columns[0], Is.EqualTo("CreateDate"));
        Assert.That(vehicleLoader.Messaging, Is.Not.Empty);
        Assert.That(vehicleLoader.Messaging!.Count, Is.EqualTo(1));
        Assert.That(vehicleLoader.Name, Is.EqualTo("VehicleLoader"));
        Assert.That(vehicleLoader.Schedule, Is.Not.Null);
        Assert.That(vehicleLoader.Schedule!.Start, Is.EqualTo("Daily at 2am UTC"));
        Assert.That(vehicleLoader.Sources, Is.Not.Empty);
        Assert.That(vehicleLoader.Sources!.Count, Is.EqualTo(1));
        var loaderSources = vehicleLoader.Sources.ToList();
        Assert.That(loaderSources[0].DataSource, Is.EqualTo("TestDataSource2"));
        Assert.That(loaderSources[0].MinimumExpectedRecords, Is.EqualTo(30));
        Assert.That(loaderSources[0].Query, Is.Not.Empty);
        
        //DataSources
        Assert.That(config.DataSources, Is.Not.Null);
        Assert.That(config.DataSources!.Count, Is.EqualTo(3));
        var testDataSource = config.DataSources.FirstOrDefault(ds => ds.Name == "TestDataSource1");
        Assert.That(testDataSource, Is.Not.Null);
        Assert.That(testDataSource!.Name, Is.EqualTo("TestDataSource1"));
        Assert.That(testDataSource.Password, Is.EqualTo("password"));
        Assert.That(testDataSource.Provider, Is.EqualTo("SqLite"));
        Assert.That(testDataSource.ConnectionString, Is.EqualTo("Data Source=Test;Mode=Memory;Cache=Shared"));
        
        //Messaging Providers
        Assert.That(config.MessagingProviders, Is.Not.Null);
        Assert.That(config.MessagingProviders!.Count, Is.EqualTo(2));
        var msgProvider = config.MessagingProviders.FirstOrDefault(mp => mp.Name == "TestMessagingProvider1");
        Assert.That(msgProvider, Is.Not.Null);
        Assert.That(msgProvider!.Name, Is.EqualTo("TestMessagingProvider1"));
        Assert.That(msgProvider.Provider, Is.EqualTo("inmemory"));
        
        //Version Control
        Assert.That(config.VersionControl, Is.Not.Null);
        Assert.That(config.VersionControl!.DefinitionFileName, Is.Not.Empty);
        Assert.That(config.VersionControl!.Provider, Is.EqualTo("azureDevOps"));
        Assert.That(config.VersionControl!.Server, Is.EqualTo("https://dev.azure.com/iwgplc"));
        Assert.That(config.VersionControl!.Project, Is.EqualTo("Nox.Test"));
        Assert.That(config.VersionControl!.Repository, Is.EqualTo("Test.Api.V1"));
        Assert.That(config.VersionControl!.RelativeProjectSourceFolder, Is.EqualTo("./"));
        Assert.That(config.VersionControl!.RelativeDockerFilePath, Is.EqualTo("/Dockerfile"));
        
        //Team
        Assert.That(config.Team, Is.Not.Null);
        Assert.That(config.Team!.DefinitionFileName, Is.Not.Empty);
        Assert.That(config.Team!.Developers, Is.Not.Null);
        var devs = config.Team.Developers!.ToList();
        Assert.That(devs[0].DefinitionFileName, Is.Not.Empty);
        Assert.That(devs[0].Name, Is.EqualTo("Test User"));
        Assert.That(devs[0].UserName, Is.EqualTo("test.user@iwgplc.com"));
        Assert.That(devs[0].MobilePhoneNumber, Is.EqualTo("+1234567890"));
        Assert.That(devs[0].IsAdmin, Is.True);
    }
}