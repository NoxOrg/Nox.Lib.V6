using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class NoxTestFixture: ConfigurationTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        TestServices.AddLogging();
        TestServices.AddNox();
        TestServices.AddSingleton<TestSourceSqlSeed>();
        TestServices.AddSingleton<TestSqlSeed>();
        TestServiceProvider = TestServices.BuildServiceProvider();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<TestSourceSqlSeed>();
        await seed.Execute();
    }
}