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
public class DropAndLoadTestFixture: GlobalTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        Environment.SetEnvironmentVariable("ENVIRONMENT", "DropAndLoad");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.DropAndLoad.json")
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<SqlHelper>();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<DropAndLoadSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        var dynamicService = TestServiceProvider.GetRequiredService<IDynamicService>();
        WaitForNoxDatabase(dynamicService, 30000);
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<DropAndLoadSeed>();
        await seed.Execute();
    }

}