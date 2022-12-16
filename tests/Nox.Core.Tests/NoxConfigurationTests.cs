using System;
using Microsoft.Extensions.Configuration;
using Nox.Core.Configuration;
using Nox.Core.Exceptions;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ConfigurationTests
{
    [Test]
    public void Must_Get_an_Exception_If_AppSettings_Are_Empty()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Empty");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxAppSettings());
        Assert.That(ex!.Message, Is.EqualTo("Could not find 'Nox:DefinitionRootPath' in environment or appsettings"));
    }
    
    [Test]
    public void Must_Get_an_Exception_If_Design_Folder_does_not_contain_a_yaml_file()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "NoService");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxAppSettings());
        Assert.That(ex!.Message, Is.EqualTo("Could not find any yaml files in /home/jan/Projects/IWG/Nox/src/Nox.Core.Tests/bin/Debug/net6.0/DesignFolders/EmptyDesign"));
    }
    
    [Test]
    public void Must_Get_an_Exception_If_Design_Folder_does_not_contain_entity_folders()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "NoEntities");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxAppSettings());
        Assert.That(ex!.Message, Is.EqualTo("Could not find any entity folders in /home/jan/Projects/IWG/Nox/src/Nox.Core.Tests/bin/Debug/net6.0/DesignFolders/NoEntities"));
    }

    [Test]
    public void Can_Load_Nox_Configuration_From_Yaml_Definitions()
    {
        var appSettings = ConfigurationHelper.GetNoxAppSettings();
        var configurator = new NoxConfigurator(appSettings!["Nox:DefinitionRootPath"]!);
        var config = configurator.LoadConfiguration();
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Name, Is.EqualTo("Test"));
        Assert.That(config.Description, Is.EqualTo("Test Microservice"));
        Assert.That(config.DefinitionFileName, Is.Not.Empty);
        Assert.That(config.EndpointProvider, Is.EqualTo(""));
        //Api
        Assert.That(config.Apis, Is.Not.Null);
        Assert.That(config.Apis!.Count, Is.EqualTo(1));                
        Assert.That(config.Apis![0].Name, Is.EqualTo("TestApi"));
        Assert.That(config.Apis![0].Description, Is.EqualTo("Test entity controller and endpoints"));
        Assert.That(config.Apis![0].DefinitionFileName, Is.Not.Empty);
        Assert.That(config.Apis![0].Routes!.Count, Is.EqualTo(3));
        Assert.That(config.Apis![0].Routes![0].Name, Is.EqualTo("Persons"));
        Assert.That(config.Apis![0].Routes![0].Description, Is.EqualTo("Returns all persons"));
        Assert.That(config.Apis![0].Routes![0].HttpVerb, Is.EqualTo("GET"));
        Assert.That(config.Apis![0].Routes![0].TargetUrl, Is.EqualTo("odata/Persons"));
        Assert.That(config.Apis![0].Routes![0].Responses!.Count, Is.EqualTo(1));
        //Database
        Assert.That(config.Database, Is.Not.Null);
        Assert.That(config.Database!.Name, Is.EqualTo("Test"));
        Assert.That(config.Database.Options, Is.EqualTo("Trusted_Connection=no;connection timeout=120;TrustServerCertificate=true"));
        Assert.That(config.Database.Password, Is.EqualTo("Developer*123"));
        Assert.That(config.Database.Server, Is.EqualTo("localhost"));
        Assert.That(config.Database.Provider, Is.EqualTo("SqlServer"));
        Assert.That(config.Database.User, Is.EqualTo("sa"));
        //Entities
        Assert.That(config.Entities, Is.Not.Null);
        Assert.That(config.Entities!.Count, Is.EqualTo(2));
        Assert.That(config.Entities![0].Attributes.Count, Is.EqualTo(3));
        Assert.That(config.Entities![0].DefinitionFileName, Is.Not.Empty);
        Assert.That(config.Entities![0].Description, Is.EqualTo("Vehicles"));
        Assert.That(config.Entities![0].Name, Is.EqualTo("Vehicle"));
        Assert.That(config.Entities![0].PluralName, Is.EqualTo("Vehicles"));
        Assert.That(config.Entities![0].RelatedParents!.Count, Is.EqualTo(1));
        Assert.That(config.Entities![0].Schema, Is.EqualTo("dbo"));
        Assert.That(config.Entities![0].Table, Is.EqualTo("Vehicle"));

        //Loaders        
        Assert.That(config.Loaders, Is.Not.Null);
        Assert.That(config.Loaders!.Count, Is.EqualTo(2));
        Assert.That(config.Loaders![0].DefinitionFileName, Is.Not.Empty);
        Assert.That(config.Loaders![0].Description, Is.EqualTo("Loads vehicle data"));
        Assert.That(config.Loaders![0].LoadStrategy, Is.Not.Null);
        Assert.That(config.Loaders![0].LoadStrategy!.Columns, Is.Not.Null);
        Assert.That(config.Loaders![0].LoadStrategy!.Columns[0], Is.EqualTo("CreateDate"));
        Assert.That(config.Loaders![0].Messaging, Is.Not.Empty);
        Assert.That(config.Loaders![0].Messaging!.Count, Is.EqualTo(1));
        Assert.That(config.Loaders![0].Name, Is.EqualTo("VehicleLoader"));
        Assert.That(config.Loaders![0].Schedule, Is.Not.Null);
        Assert.That(config.Loaders![0].Schedule!.Start, Is.EqualTo("Daily at 2am UTC"));
        Assert.That(config.Loaders![0].Sources, Is.Not.Empty);
        Assert.That(config.Loaders![0].Sources!.Count, Is.EqualTo(1));
        Assert.That(config.Loaders![0].Sources![0].DataSource, Is.EqualTo("TestDataSource2"));
        Assert.That(config.Loaders![0].Sources![0].MinimumExpectedRecords, Is.EqualTo(30));
        Assert.That(config.Loaders![0].Sources![0].Query, Is.Not.Empty);
        
        //DataSources
        Assert.That(config.DataSources, Is.Not.Null);
        Assert.That(config.DataSources!.Count, Is.EqualTo(3));
        Assert.That(config.DataSources![0].Name, Is.EqualTo("TestDataSource1"));
        Assert.That(config.DataSources![0].Password, Is.EqualTo("password"));
        Assert.That(config.DataSources![0].Provider, Is.EqualTo("TestProvider"));
        Assert.That(config.DataSources![0].Server, Is.EqualTo("localhost"));
        Assert.That(config.DataSources![0].User, Is.EqualTo("user"));
        
        //Messaging Providers
        Assert.That(config.MessagingProviders, Is.Not.Null);
        Assert.That(config.MessagingProviders!.Count, Is.EqualTo(2));
        Assert.That(config.MessagingProviders![0].Name, Is.EqualTo("TestMessagingProvider1"));
        Assert.That(config.MessagingProviders![0].Provider, Is.EqualTo("InMemory"));
    }
    
}