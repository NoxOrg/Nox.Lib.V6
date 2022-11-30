using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Data;
using Nox.Messaging;
using Nox.Microservice;
using Nox.Microservice.Extensions;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class MessagingTestFixture
{
    protected IServiceCollection? TestServiceCollection;
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        TestServiceCollection = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        TestServiceCollection.AddSingleton<IConfiguration>(config);
        TestServiceCollection.AddLogging();
        TestServiceCollection
            .AddDynamicModel()
            .AddDynamicService();
    }

    public void BuildServiceProvider()
    {
        TestServiceProvider = TestServiceCollection!.BuildServiceProvider();
    }
}