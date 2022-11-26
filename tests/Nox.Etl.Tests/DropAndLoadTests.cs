using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Etl.Tests;

public class DropAndLoadTests: DropAndLoadTestFixture
{
    [Test]
    public async Task Can_Execute_a_DropAndLoad()
    {
        var loaderExecutor = TestServiceProvider!.GetRequiredService<ILoaderExecutor>();
        var service = TestServiceProvider!.GetRequiredService<IDynamicService>();
        var loader = service.Loaders.Single(l => l.Name == "VehicleLoader");
        var entity = service.Entities.FirstOrDefault(e => e.Key == "Vehicle").Value;
        var result = await loaderExecutor.ExecuteLoaderAsync(loader, service.ServiceDatabase.DatabaseProvider!, entity);
        Assert.That(result, Is.True);
        var sqlHelper = TestServiceProvider!.GetRequiredService<SqlHelper>();
        var count = await sqlHelper.ExecuteInt("SELECT COUNT(*) FROM dbo.Vehicle");
        Assert.That(count, Is.EqualTo(1000));
    }
}