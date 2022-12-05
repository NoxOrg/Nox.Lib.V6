using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Data;
using Nox.Microservice;
using Nox.Microservice.Extensions;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DataTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services
            .AddDatabaseProviderFactory()
            .AddDbContext<IDynamicDbContext, DynamicDbContext>()
            .AddSingleton<IDynamicModel, DynamicModel>()
            .AddSingleton<IDynamicService, DynamicService>();
        TestServiceProvider = services.BuildServiceProvider();
    }
}