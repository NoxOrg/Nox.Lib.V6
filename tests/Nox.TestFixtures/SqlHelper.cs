using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures;

public class SqlHelper
{
    private readonly SqliteConnection _connection;

    public SqlHelper(IConfiguration config)
    {
        _connection = new SqliteConnection(config["ConnectionString:Test"]);
    }
    
    public async Task ExecuteAsync(string sql)
    {
        await _connection.OpenAsync();
        var cmd = new SqliteCommand()
        {
            Connection = _connection,
            CommandText = sql,
        };
        await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
    }

    public async Task<int?> ExecuteInt(string sql)
    {
        int? result = null;
        try
        {
            await _connection.OpenAsync();
            var cmd = new SqliteCommand
            {
                Connection = _connection,
                CommandText = sql,
            };
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                result = reader.GetInt32(0);
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }

        return result;
    }

    public async Task<DataTable> ExecuteTable(string sql)
    {
        var result = new DataTable();
        try
        {
            await _connection.OpenAsync();
            var cmd = new SqliteCommand
            {
                Connection = _connection,
                CommandText = sql,
            };
            var reader = await cmd.ExecuteReaderAsync();
            result.Load(reader);
        }
        finally
        {
            await _connection.CloseAsync();
        }

        return result;
    }
}