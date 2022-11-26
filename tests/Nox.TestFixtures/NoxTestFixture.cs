using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Microservice;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class NoxTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "");
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<TestSourceSqlSeed>();
        services.AddSingleton<TestSqlSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<TestSourceSqlSeed>();
        await seed.Execute();
    }
}