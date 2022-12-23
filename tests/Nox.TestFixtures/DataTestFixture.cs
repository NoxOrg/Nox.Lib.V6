using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.TestFixtures.Seeds;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DataTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void DataTestFixtureSetup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "Data");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Data.json")
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<SqlHelper>();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<DataTestSqlSeed>();
        TestServiceProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void TestSetup()
    {
        if (File.Exists("./DataTest.db")) File.Delete("./DataTest.db");
    }
}