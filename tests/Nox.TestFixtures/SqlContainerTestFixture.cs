using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Builders;
using NUnit.Framework;

namespace Nox.TestFixtures;

[TestFixture]
public class SqlContainerTestFixture : DataTestFixture
{
    [OneTimeSetUp]
    public void SqlContainerFixtureSetup()
    {
        using var vol = new Builder().UseVolume("sqlVol").RemoveOnDispose().Build();
        using (var container = new Builder()
                   .UseContainer()
                   .UseImage("mcr.microsoft.com/azure-sql-edge:latest")
                   .ExposePort(1433)
                   .AsUser("root")
                   .WithEnvironment("SA_PASSWORD=Developer*123", "ACCEPT_EULA=Y", "MSSQL_PID=Developer")
                   .MountVolume(vol, "/var/opt/mssql/data", MountType.ReadWrite)
                   .WaitForPort("1433/tcp", 10000)
                   .Build()
                   .Start())
        {
            
        }
    }

}