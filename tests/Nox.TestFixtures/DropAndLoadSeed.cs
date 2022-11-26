using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures;

public class DropAndLoadSeed
{
    private readonly SqlConnection _con;

    public DropAndLoadSeed(IConfiguration configuration)
    {
        _con = new SqlConnection(configuration["ConnectionString:Master"]);
    }

    public async Task Execute()
    {
        await _con.OpenAsync();
        await RunScript("DROP DATABASE IF EXISTS TestSource;");
        await RunScript("CREATE DATABASE TestSource;");
        await RunScript("use TestSource");
        await RunScript("CREATE TABLE SourceVehicle (Id int NOT NULL PRIMARY KEY IDENTITY(1,1), VehicleBrand varchar(40) NOT NULL, VehicleColor varchar(10) NOT NULL);");
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
        var cmd = new SqlCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
}