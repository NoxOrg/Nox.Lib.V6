using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data;
using Nox.Lib;
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
            .AddDbContext<IDynamicDbContext, DynamicDbContext>()
            .AddSingleton<IDynamicModel, DynamicModel>()
            .AddSingleton<IDynamicService, DynamicService>();
        services.AddSingleton<SqlHelper>();
        TestServiceProvider = services.BuildServiceProvider();
    }
}