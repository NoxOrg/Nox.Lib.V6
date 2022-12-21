using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Database;
using Nox.Data;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DataTestFixture: GlobalTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public void DataTestFixtureSetup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<SqlHelper>();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<TestSqlSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        var dynamicService = TestServiceProvider.GetRequiredService<IDynamicService>();
        WaitForNoxDatabase(dynamicService, 30000);
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
    }
}