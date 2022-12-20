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
public class DropAndLoadTestFixture: ConfigurationTestFixture
{
    protected IServiceProvider? TestServiceProvider;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        TestServices.AddLogging();
        TestServices.AddNox();
        TestServices.AddSingleton<SqlHelper>();
        
        TestServices.AddSingleton<DropAndLoadSeed>();
        TestServiceProvider = TestServices.BuildServiceProvider();
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        await sqlHelper.ExecuteAsync("DELETE FROM Vehicle");
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var seed = TestServiceProvider.GetRequiredService<DropAndLoadSeed>();
        await seed.Execute();
    }

}