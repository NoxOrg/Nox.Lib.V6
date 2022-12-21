using System;
using System.Linq;
using System.Threading;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using NUnit.Framework;

namespace Nox.TestFixtures;

[SetUpFixture]
public class GlobalTestFixture
{
    private ICompositeService? _dockerService;
  
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Console.WriteLine("Running Global Setup");
        if (_dockerService == null || _dockerService.Containers.All(c => c.Name != "sqlserver_container"))
        {
            AddSqlServerContainer();            
        }
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        if (_dockerService != null && _dockerService.Containers.Any(c => c.Name == "sqlserver_container"))
        {
            _dockerService.Stop();
            _dockerService.Remove();    
        }
    }

    private void AddSqlServerContainer()
    {
        const string dockerFile = "docker-compose.yml";
        _dockerService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(dockerFile)
            .RemoveOrphans()
            .WaitForPort("sqlserver", "1433/tcp", 10000)
            .Build()
            .Start();
    }
    
    public void WaitForNoxDatabase(IDynamicService dynamicService, int timeoutMs)
    {
        var con = new SqlConnection(dynamicService.MetaService.Database!.ConnectionString);
        var cts = new CancellationTokenSource(timeoutMs);
        cts.Token.Register(() =>
        {
            if (!cts.IsCancellationRequested) throw new Exception("Unable to get a response from Nox Database");
        });
        while (!cts.IsCancellationRequested)
        {
            try
            {
                con.Open();
                var cmd = new SqlCommand("SELECT 1");
                cmd.Connection = con;
                cmd.CommandTimeout = 5;
                cmd.ExecuteReader();
                con.Close();
                cts.Cancel();
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }
    }
}