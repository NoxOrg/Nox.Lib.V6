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
public class DropAndLoadTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        if (File.Exists("./DropAndLoadMaster.db")) File.Delete("./DropAndLoadMaster.db");
        if (File.Exists("./DropAndLoad.db")) File.Delete("./DropAndLoad.db");
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
        TestServiceProvider.GetRequiredService<IDynamicService>();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        var seed = TestServiceProvider.GetRequiredService<DropAndLoadSeed>();
        await seed.Execute();
    }

}