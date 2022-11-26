using System;
using Microsoft.Extensions.Configuration;
using Nox.Core.Configuration;
using Nox.Core.Exceptions;
using NUnit.Framework;

namespace Nox.Core.Tests;

public class ConfigurationTests
{
    [Test]
    public void Can_Load_Nox_Configuration()
    {
        var config = ConfigurationHelper.GetNoxConfiguration();
        Assert.AreEqual("Endpoint=sb://nox-msg.servicebus.windows.net/;SharedAccessKeyName=ApplicationTesterKey;SharedAccessKey=oXL4Rvc2xqs7krGy0DQLeNuy0hohSfp2uKUfold/bYo=", config!.GetValue<string>("ConnectionString:AzureServiceBus"));
        Assert.AreEqual("user id=data_factory_service;password=d4t4_f4ct0ry_s3rv1c3;server=10.255.187.20,1433;database=TitanProduction;Trusted_Connection=no;connection timeout=120;ApplicationIntent=ReadOnly;", config!.GetValue<string>("ConnectionString:MasterDataSource"));
        Assert.AreEqual("569k1ec2qht2fu10u4s4hdno0p", config!.GetValue<string>("XECurrency:ApiPassword"));
        Assert.AreEqual("iwgplc510889972", config!.GetValue<string>("XECurrency:ApiUser"));
        Assert.AreEqual("2023-11-01|ENTERPRISE|IW047|IWG|Andre Sharpe|andre.sharpe@iwgplc.com||nKrp+FFlN2NRnpFEI0QObDQuFZs+jsuTrafG5wr7p+ZHYN13S+nbx5/pOpK9Kmnc33CCw5YlltdZ/mcd2gBEUGlW0Pi1/QsBq/IZBHRUuJfFztpOi6F6jWpAOSyMJaYn9scNY+daCp4cMpFHIeS6du0mUOatPWwjaTgA7s4Iv4U=", config!.GetValue<string>("EtlBox:LicenseKey"));
        Assert.AreEqual("inmemory", config!.GetValue<string>("ServiceMessageBusProvider"));
        Assert.AreEqual("", config!.GetValue<string>("ServiceMessageBusConnectionVariable"));
    }

    [Test]
    public void Must_Get_an_Exception_If_AppSettings_Are_Empty()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Empty");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxConfiguration());
        Assert.That(ex!.Message, Is.EqualTo("Could not find 'Nox:DefinitionRootPath' in environment or appsettings"));
    }
    
    [Test]
    public void Must_Get_an_Exception_If_Design_Folder_does_not_contain_a_yaml_file()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "NoService");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxConfiguration());
        Assert.That(ex!.Message, Is.EqualTo("Could not find any yaml files in /home/jan/Projects/IWG/Nox/src/Nox.Core.Tests/bin/Debug/net6.0/DesignFolders/EmptyDesign"));
    }
    
    [Test]
    public void Must_Get_an_Exception_If_Design_Folder_does_not_contain_entity_folders()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "NoEntities");
        var ex = Assert.Throws<ConfigurationException>(() => ConfigurationHelper.GetNoxConfiguration());
        Assert.That(ex!.Message, Is.EqualTo("Could not find any entity folders in /home/jan/Projects/IWG/Nox/src/Nox.Core.Tests/bin/Debug/net6.0/DesignFolders/NoEntities"));
    }
    
}