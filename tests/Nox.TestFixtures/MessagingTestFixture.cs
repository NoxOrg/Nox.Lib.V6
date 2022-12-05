using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Lib;
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
            .AddSingleton<IDynamicModel, DynamicModel>()
            .AddSingleton<IDynamicService, DynamicService>();
    }

    public void BuildServiceProvider()
    {
        TestServiceProvider = TestServiceCollection!.BuildServiceProvider();
    }
}