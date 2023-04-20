using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;
using Nox.Core.Models;
using Nox.Data.Csv;

namespace Nox.DataProvider.Tests;

public class CsvDataProviderTests
{
    [Test]
    public void Can_Add_Data_Provider_To_Service_Collection()
    {
        var services = new ServiceCollection();
        services.AddCsvDataProvider();
        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IDataProvider>();
        Assert.That(svc, Is.Not.Null);
        Assert.That(svc, Is.InstanceOf<CsvDataProvider>());
    }

    [Test]
    public void Can_Configure_Data_Provider_From_Path()
    {
        const string options = "Path=./TestFiles/Empty.csv"; 
        var provider = new CsvDataProvider();
        var db = new ServiceDatabase
        {
            Options = options
        };
        provider.Configure(db, "Test");
        Assert.Multiple(() =>
        {
            Assert.That(provider.ConnectionString, Is.EqualTo(options));
            Assert.That(provider.Name, Is.EqualTo("csv"));
        });
    }

    [Test]
    public void Must_Get_Exception_If_Options_Not_Set()
    {
        const string options = "Street=./TestFiles/Empty.csv"; 
        var provider = new CsvDataProvider();
        var db = new ServiceDatabase
        {
            Options = options
        };
        Assert.Throws<Exception>(delegate { provider.Configure(db, "Test"); });
    }

    [Test]
    public void Can_Create_a_Data_Flow_Source()
    {
        
    }
    
}