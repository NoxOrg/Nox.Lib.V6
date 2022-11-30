using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;
using Nox.Microservice.Extensions;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class MergeNewTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        Environment.SetEnvironmentVariable("ENVIRONMENT", "MergeNew");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.MergeNew.json")
            .Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<SqlHelper>();
        services.AddSingleton<MergeNewSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        await sqlHelper.ExecuteAsync("Delete FROM meta.MergeState");
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<MergeNewSeed>();
        await seed.Execute();
    }
}