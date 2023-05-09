using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;
using Nox.TestFixtures;
using Nox.TestFixtures.Seeds;
using NUnit.Framework;

namespace Nox.Etl.MergeNew.Tests;

public class MergeNewTests : MergeNewTestFixture
{
    [Test]
    public async Task Can_Execute_a_MergeNew()
    {
        var loaderExecutor = TestServiceProvider!.GetRequiredService<IEtlExecutor>();
        var service = TestServiceProvider!.GetRequiredService<IDynamicService>();
        var loader = service.Loaders!.Single(l => l.Name == "VehicleLoader");
        var entity = service.Entities!.FirstOrDefault(e => e.Key == "Vehicle").Value;
        var result = await loaderExecutor.ExecuteLoaderAsync(service.MetaService, loader, entity);
        Assert.That(result, Is.True);
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        var count = await sqlHelper.ExecuteInt("SELECT COUNT(*) FROM Vehicle");
        Assert.That(count, Is.EqualTo(100));
        //seed 10 more rows in Source
        var seed = TestServiceProvider!.GetRequiredService<MergeNewSeed>();
        await seed.Update(10);
        await seed.Insert(false, 10);
        result = await loaderExecutor.ExecuteLoaderAsync(service.MetaService, loader, entity);
        Assert.That(result, Is.True);
        count = await sqlHelper.ExecuteInt("SELECT COUNT(*) FROM Vehicle");
        //Count must be 110 as only 10 new items
        Assert.That(count, Is.EqualTo(110));
    }
}