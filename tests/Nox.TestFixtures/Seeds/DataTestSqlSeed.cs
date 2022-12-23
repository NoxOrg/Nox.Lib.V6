using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures.Seeds;

public class DataTestSqlSeed
{
    private readonly SqliteConnection _con;

    public DataTestSqlSeed(IConfiguration configuration)
    {
        _con = new SqliteConnection(configuration["ConnectionString:Test"]);
    }

    public async Task Execute()
    {
        await _con.OpenAsync();
        await RunScript("DELETE FROM Vehicle;");
        await RunScript("DELETE FROM Person;");
        await SeedPerson(1);
        await _con.CloseAsync();
    }

    private async Task SeedPerson(int id)
    {
        var faker = new Faker();
        await RunScript($"INSERT INTO Person (Id, Name, Age) VALUES({id}, 'Test Person', '{faker.Random.Int(10, 100)}')");
        await SeedVehicles(id);
    }
    
    private async Task SeedVehicles(int personId)
    {
        var faker = new Faker();
        for (var i = 0; i < 5; i++)
        {
            await RunScript($"INSERT INTO Vehicle (Id, PersonId, Brand, Color) VALUES({i}, {personId}, '{faker.Random.String2(40)}', '{faker.Random.String2(10)}')");    
        }
    }

    private async Task RunScript(string script)
    {
        var cmd = new SqliteCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
}