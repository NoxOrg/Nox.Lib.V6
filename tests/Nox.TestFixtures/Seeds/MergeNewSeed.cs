using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures.Seeds;

public class MergeNewSeed
{
    private readonly SqliteConnection _con;

    public MergeNewSeed(IConfiguration configuration)
    {
        _con = new SqliteConnection(configuration["ConnectionString:MasterDataSource"]);
    }

    public async Task Insert(bool drop = true, int rowCount = 100)
    {
        await _con.OpenAsync();
        if (drop)
        {
            await RunScript("CREATE TABLE SourceVehicle (Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, VehicleBrand text NOT NULL, VehicleColor text NOT NULL, CreateDate text, EditDate text);");    
        }
        
        for (var row = 0; row < rowCount; row++)
        {
            await SeedVehicle(DateTime.Now, DateTime.Now);
        }
        await _con.CloseAsync();
    }

    public async Task Update(int rowCount)
    {
        await _con.OpenAsync();
        await RunScript($"UPDATE SourceVehicle SET EditDate = '{DateTime.Now}' WHERE Id < 11");
        await _con.CloseAsync();
    }

    
    private async Task SeedVehicle(DateTime createDate, DateTime editDate)
    {
        var faker = new Faker();
        await RunScript($"INSERT INTO SourceVehicle (VehicleBrand, VehicleColor, CreateDate, EditDate) VALUES('{faker.Random.String2(40)}', '{faker.Random.String2(10)}', '{createDate.ToString()}', '{editDate.ToString()}')");
    }

    private async Task RunScript(string script)
    {
        var cmd = new SqliteCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
}