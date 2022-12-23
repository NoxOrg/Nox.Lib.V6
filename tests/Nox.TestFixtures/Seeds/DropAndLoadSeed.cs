using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures.Seeds;

public class DropAndLoadSeed
{
    private readonly SqliteConnection _con;

    public DropAndLoadSeed(IConfiguration configuration)
    {
        _con = new SqliteConnection(configuration["ConnectionString:MasterDataSource"]);
    }

    public async Task Execute()
    {
        await _con.OpenAsync();
        await RunScript("CREATE TABLE SourceVehicle (Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, VehicleBrand text NOT NULL, VehicleColor text NOT NULL);");
        for (var i = 0; i < 1000; i++)
        {
            await SeedVehicle();
            
        }
        await _con.CloseAsync();
    }

    
    private async Task SeedVehicle()
    {
        var faker = new Faker();
        await RunScript($"INSERT INTO SourceVehicle (VehicleBrand, VehicleColor) VALUES('{faker.Random.String2(40)}', '{faker.Random.String2(10)}')");    
    }

    private async Task RunScript(string script)
    {
        var cmd = new SqliteCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
}