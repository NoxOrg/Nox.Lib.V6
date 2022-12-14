using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Models;
using Nox.TestFixtures;
using NUnit.Framework;

namespace Nox.Data.Tests;

[TestFixture]
public class SqlServerDatabaseProviderTests : DataTestFixture
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
        var factory = TestServiceProvider!.GetRequiredService<IDatabaseProviderFactory>();
        var provider = factory.Create("sqlserver");
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
                Assert.AreEqual($"nvarchar({width})", result);
                break;
            case "varchar":
                Assert.AreEqual($"varchar({width})", result);
                break;
            case "nvarchar":
                Assert.AreEqual($"nvarchar({width})", result);
                break;
            case "url":
                Assert.AreEqual("varchar(2048)", result);
                break;
            case "email":
                Assert.AreEqual("varchar(320)", result);
                break;
            case "char":
                Assert.AreEqual($"nchar({width})", result);
                break;
            case "guid":
                Assert.AreEqual($"uniqueidentifier", result);
                break;
            case "date":
                Assert.AreEqual($"date", result);
                break;
            case "datetime":
                Assert.AreEqual($"datetimeoffset", result);
                break;
            case "time":
                Assert.AreEqual($"datetime", result);
                break;
            case "timespan":
                Assert.AreEqual($"timespan", result);
                break;
            case "bool":
            case "boolean":
                Assert.AreEqual($"bit", result);
                break;
            case "object":
                Assert.AreEqual($"sql_variant", result);
                break;
            case "int":
                Assert.AreEqual($"int", result);
                break;
            case "uint":
                Assert.AreEqual($"uint", result);
                break;
            case "bigint":
                Assert.AreEqual($"bigint", result);
                break;
            case "smallint":
                Assert.AreEqual($"smallint", result);
                break;
            case "decimal":
            case "money":
            case "smallmoney":
                Assert.AreEqual($"decimal({width},{precision})", result);
                break;
            default:
                Assert.AreEqual($"nvarchar(max)", result);
                break;
        }
    }

    [Test]
    public void Can_Convert_a_Table_Name()
    {
        var factory = TestServiceProvider!.GetRequiredService<IDatabaseProviderFactory>();
        var provider = factory.Create("sqlserver");
        var result = provider.ToTableNameForSql("MyTable", "MySchema");
        Assert.AreEqual("[MySchema].[MyTable]", result);
    }
    
    [Test]
    public void Can_Convert_a_raw_Table_Name()
    {
        var factory = TestServiceProvider!.GetRequiredService<IDatabaseProviderFactory>();
        var provider = factory.Create("sqlserver");
        var result = provider.ToTableNameForSqlRaw("MyTable", "MySchema");
        Assert.AreEqual("MySchema.MyTable", result);
    }
}