using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Lib;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class DropAndLoadTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        Environment.SetEnvironmentVariable("ENVIRONMENT", "DropAndLoad");
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.DropAndLoad.json")
            .Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddNox();
        services.AddSingleton<SqlHelper>();
        
        services.AddSingleton<DropAndLoadSeed>();
        TestServiceProvider = services.BuildServiceProvider();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<DropAndLoadSeed>();
        await seed.Execute();
    }

}