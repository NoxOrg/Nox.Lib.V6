using System;
using System.Collections.Immutable;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DataTestFixture: ConfigurationTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void DataTestFixtureSetup()
    {
        TestServices.AddLogging();
        TestServices.AddNox();
        TestServices.AddSingleton<TestSqlSeed>();
    }

    [OneTimeTearDown]
    public void DataTestFixtureTeardown()
    {
        var dockerService = TestServiceProvider!.GetService<ICompositeService>();
        if (dockerService != null)
        {
            dockerService.Stop();
            dockerService.Remove();    
        }
    }

    public void BuildServiceProvider()
    {
        TestServiceProvider = TestServices.BuildServiceProvider();
    }

    public void AddSqlServerContainer()
    {
        const string dockerFile = "docker-compose.yml";
        var svc = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(dockerFile)
            .RemoveOrphans()
            .WaitForPort("sqlserver", "1433/tcp", 10000)
            .Build()
            .Start();
        TestServices.AddSingleton<ICompositeService>(svc);
    }
}