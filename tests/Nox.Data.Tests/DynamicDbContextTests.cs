using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Configuration;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Data.Tests;

public class DynamicDbContextTests: DataTestFixture
{
    [Test]
    public void Can_Get_a_Dynamic_Collection()
    {
        var context = TestServiceProvider!.GetRequiredService<DynamicDbContext>();
        var dynamicQueryable = context.GetDynamicCollection("People");
        Assert.NotNull(dynamicQueryable);
        Assert.AreEqual("Person, DynamicPoco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", dynamicQueryable.ElementType.AssemblyQualifiedName);
        dynamicQueryable = context.GetDynamicCollection("Vehicles");
        Assert.IsNotNull(dynamicQueryable);
        Assert.AreEqual("Vehicle, DynamicPoco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", dynamicQueryable.ElementType.AssemblyQualifiedName);                    
    }

    [Test]
    public void Can_Get_Dynamic_Single_Result()
    {
        var context = TestServiceProvider!.GetRequiredService<DynamicDbContext>();
        var result = context.GetDynamicSingleResult("People", 1);
        Assert.NotNull(result);
        Assert.AreEqual("Microsoft.AspNetCore.OData.Results.SingleResult`1[[Person, DynamicPoco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]", result.GetType().FullName);
    }
    
    [Test]
    public async Task Can_Get_Dynamic_Navigation_and_property()
    {
        TestServiceProvider!.GetRequiredService<IDynamicModel>();
        var testSeed = TestServiceProvider!.GetRequiredService<TestSqlSeed>();
        await testSeed.Execute();
        var context = TestServiceProvider!.GetRequiredService<DynamicDbContext>();
        var result = context.GetDynamicNavigation("Vehicles", 1, "Person");
        Assert.NotNull(result); 
        Assert.AreEqual("Person, DynamicPoco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", result.GetType().AssemblyQualifiedName);
        var prop = context.GetDynamicObjectProperty("People", 1, "Name");
        Assert.NotNull(prop);
        Assert.IsInstanceOf<string>(prop);
        Assert.AreEqual("Test Person", prop);
    }

    [Test]
    public async Task Can_Post_a_Dynamic_Object()
    {
        var testSeed = TestServiceProvider!.GetRequiredService<TestSqlSeed>();
        await testSeed.Execute();
        var context = TestServiceProvider!.GetRequiredService<DynamicDbContext>();
        context.PostDynamicObject("People", "{\"Id\": 2, \"Name\": \"Another Test\", \"Age\": 100}");
        var config = TestServiceProvider!.GetRequiredService<IConfiguration>();
        var con = new SqlConnection(config["ConnectionString:Test"]);
        await con.OpenAsync();
        var sql = "SELECT * FROM dbo.Person WHERE Id = 2";
        var cmd = new SqlCommand(sql)
        {
            Connection = con
        };
        var reader = await cmd.ExecuteReaderAsync();
        Assert.That(reader.HasRows, Is.True);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetString(1), Is.EqualTo("Another Test"));
        Assert.That(reader.GetInt32(2), Is.EqualTo(100));
        await con.CloseAsync();
    }
}