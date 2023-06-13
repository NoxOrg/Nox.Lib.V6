using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures.Seeds;

public class TestSourceSqlSeed
{
    private readonly SqliteConnection _con;

    public TestSourceSqlSeed(IConfiguration configuration)
    {
        _con = new SqliteConnection(configuration["ConnectionString:Master"]);
    }

    public async Task Execute()
    {
        await _con.OpenAsync();
        await RunScript("CREATE TABLE SourcePerson (Id int NOT NULL PRIMARY KEY, PersonName varchar(100) NOT NULL, PersonAge int NOT NULL);");
        await RunScript("CREATE TABLE SourceVehicle (Id int NOT NULL PRIMARY KEY IDENTITY(1,1), PersonId int NOT NULL, VehicleBrand varchar(40) NOT NULL, VehicleColor varchar(10) NOT NULL);");
        for (var i = 0; i < 100; i++)
        {
            await SeedPerson(i);
            
        }
        await _con.CloseAsync();
    }

    private async Task SeedPerson(int id)
    {
        var faker = new Faker();
        await RunScript($"INSERT INTO SourcePerson (Id, PersonName, PersonAge) VALUES({id}, '{faker.Random.String2(100)}', '{faker.Random.Int(10, 100)}')");
        await SeedVehicles(id);
    }
    
    private async Task SeedVehicles(int personId)
    {
        var faker = new Faker();
        for (var i = 0; i < 5; i++)
        {
            await RunScript($"INSERT INTO SourceVehicle (PersonId, VehicleBrand, VehicleColor) VALUES({personId}, '{faker.Random.String2(40)}', '{faker.Random.String2(10)}')");    
        }
    }

    private async Task RunScript(string script)
    {
        var cmd = new SqliteCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
    
}