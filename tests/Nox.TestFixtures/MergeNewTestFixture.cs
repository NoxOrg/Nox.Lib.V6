using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Lib;
using Nox.TestFixtures.Seeds;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class MergeNewTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        if (File.Exists("./MergeNewMaster.db")) File.Delete("./MergeNewMaster.db");
        if (File.Exists("./MergeNew.db")) File.Delete("./MergeNew.db");
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
        var svc = TestServiceProvider.GetRequiredService<IDynamicService>();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        await sqlHelper.ExecuteAsync("DELETE FROM MergeState");
        await sqlHelper.ExecuteAsync("INSERT INTO Person (Id, Name, Age) VALUES(0, 'Test Person', 10)");
        var seed = TestServiceProvider.GetRequiredService<MergeNewSeed>();
        await seed.Insert();
    }
}