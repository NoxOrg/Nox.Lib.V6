using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Etl.Tests;

public class MergeNewTests : MergeNewTestFixture
{
    [Test]
    public async Task Can_Execute_a_MergeNew()
    {
        var loaderExecutor = TestServiceProvider!.GetRequiredService<IEtlExecutor>();
        var metaService = TestServiceProvider!.GetRequiredService<IMetaService>();
        var service = TestServiceProvider!.GetRequiredService<IDynamicService>();
        var loader = service.Loaders!.Single(l => l.Name == "VehicleLoader");
        var entity = service.Entities!.FirstOrDefault(e => e.Key == "Vehicle").Value;
        var result = await loaderExecutor.ExecuteLoaderAsync(metaService, loader, entity);
        Assert.That(result, Is.True);
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        var count = await sqlHelper.ExecuteInt("SELECT COUNT(*) FROM dbo.Vehicle");
        Assert.That(count, Is.EqualTo(100));
        //seed 100 rows in Source
        var seed = TestServiceProvider!.GetRequiredService<MergeNewSeed>();
        await seed.Execute(false);
        Console.WriteLine("Seeded 100 more rows");
        result = await loaderExecutor.ExecuteLoaderAsync(metaService, loader, entity);
        Assert.That(result, Is.True);
        count = await sqlHelper.ExecuteInt("SELECT COUNT(*) FROM dbo.Vehicle");
        //Count must be 120 as only 20 of the new seeded items has a create/edit date > last merge date
        Assert.That(count, Is.EqualTo(120));
    }
}