using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Data;
using Nox.Microservice.Extensions;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class EtlTestFixture
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
            .AddDynamicDbContext()
            .AddDynamicModel()
            .AddDynamicService();
        services.AddSingleton<SqlHelper>();
        TestServiceProvider = services.BuildServiceProvider();
    }
}