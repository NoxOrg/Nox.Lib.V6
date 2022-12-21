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
public class MergeNewTestFixture: GlobalTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "MergeNew");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.MergeNew.json")
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<SqlHelper>();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<MergeNewSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        var dynamicService = TestServiceProvider.GetRequiredService<IDynamicService>();
        WaitForNoxDatabase(dynamicService, 30000);
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        await sqlHelper.ExecuteAsync("DELETE FROM Person");
        await sqlHelper.ExecuteAsync("Delete FROM meta.MergeState");
        await sqlHelper.ExecuteAsync("INSERT INTO PERSON (Id, Name, Age) VALUES(0, 'Test Person', 10)");
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<MergeNewSeed>();
        await seed.Insert();
    }
}