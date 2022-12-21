using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures;

public class MergeNewSeed
{
    private readonly SqlConnection _con;

    public MergeNewSeed(IConfiguration configuration)
    {
        _con = new SqlConnection(configuration["ConnectionString:Master"]);
    }

    public async Task Insert(bool drop = true, int rowCount = 100)
    {
        await _con.OpenAsync();
        if (drop)
        {
            await RunScript("DROP DATABASE IF EXISTS TestSource;");
            await RunScript("CREATE DATABASE TestSource;");
            await RunScript("use TestSource");
            await RunScript("CREATE TABLE SourceVehicle (Id int NOT NULL PRIMARY KEY IDENTITY(1,1), VehicleBrand varchar(40) NOT NULL, VehicleColor varchar(10) NOT NULL, CreateDate datetime2, EditDate datetime2);");    
        }
        else
        {
            await RunScript("use TestSource");
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
        await RunScript("use TestSource");
        await RunScript($"UPDATE TOP ({rowCount}) dbo.SourceVehicle SET EditDate = '{DateTime.Now}'");
        await _con.CloseAsync();
    }

    
    private async Task SeedVehicle(DateTime createDate, DateTime editDate)
    {
        var faker = new Faker();
        await RunScript($"INSERT INTO SourceVehicle (VehicleBrand, VehicleColor, CreateDate, EditDate) VALUES('{faker.Random.String2(40)}', '{faker.Random.String2(10)}', '{createDate.ToString()}', '{editDate.ToString()}')");
    }

    private async Task RunScript(string script)
    {
        var cmd = new SqlCommand(script);
        cmd.Connection = _con;
        await cmd.ExecuteNonQueryAsync();
    }
}