using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Models;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Data.Tests;

public class SqLiteDatabaseProviderTests: DataTestFixture
{
    [Test]
    [TestCase("string", 10)]
    [TestCase("varchar", 20)]
    [TestCase("nvarchar", 30)]
    [TestCase("url")]
    [TestCase("email")]
    [TestCase("char", 40)]
    [TestCase("guid")]
    [TestCase("date")]
    [TestCase("datetime")]
    [TestCase("time")]
    [TestCase("timespan")]
    [TestCase("object")]
    [TestCase("bool")]
    [TestCase("boolean")]
    [TestCase("object")]
    [TestCase("int")]
    [TestCase("uint")]
    [TestCase("bigint")]
    [TestCase("smallint")]
    [TestCase("decimal", 10, 2)]
    [TestCase("money", 14,2)]
    [TestCase("smallmoney", 8, 2)]
    [TestCase("unknown")]
    public void Can_Convert_Database_Column_Type(string typeName, int width = 0, int precision = 0)
    {
        var factory = TestServiceProvider!.GetRequiredService<IDataProviderFactory>();
        var provider = factory.Create(DataProvider.SqLite);
        var attr = new EntityAttribute
        {
            Name = "DbColumn",
            Description = "Test Column",
            Type = typeName,
            MaxWidth = width
        };
        var result = provider.ToDatabaseColumnType(attr);
        switch (typeName)
        {
            case "string":
            case "varchar":
            case "nvarchar":
            case "url":
            case "email":
            case "char":
            case "guid":
            case "date":
            case "datetime":
            case "time":
            case "timespan":
                Assert.AreEqual("text", result);
                break;
            case "bool":
            case "boolean":
            case "int":
            case "uint":
            case "bigint":
            case "smallint":
                Assert.AreEqual($"integer", result);
                break;
            case "object":
                Assert.IsNull(result);
                break;
            case "decimal":
            case "money":
            case "smallmoney":
                Assert.AreEqual("real", result);
                break;
            default:
                Assert.AreEqual($"text", result);
                break;
        }
    }

    [Test]
    public void Can_Convert_a_Table_Name()
    {
        var factory = TestServiceProvider!.GetRequiredService<IDataProviderFactory>();
        var provider = factory.Create(DataProvider.SqLite);
        var result = provider.ToTableNameForSql("MyTable", "MySchema");
        Assert.AreEqual("[MyTable]", result);
    }
    
    [Test]
    public void Can_Convert_a_raw_Table_Name()
    {
        var factory = TestServiceProvider!.GetRequiredService<IDataProviderFactory>();
        var provider = factory.Create(DataProvider.SqLite);
        var result = provider.ToTableNameForSqlRaw("MyTable", "MySchema");
        Assert.AreEqual("MyTable", result);
    }
}