using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Nox.TestFixtures;

public class SqlHelper
{
    private readonly SqlConnection _connection;

    public SqlHelper(IConfiguration config)
    {
        _connection = new SqlConnection(config["ConnectionString:Test"]);
    }
    
    public async Task ExecuteAsync(string sql)
    {
        await _connection.OpenAsync();
        var cmd = new SqlCommand
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
            var cmd = new SqlCommand
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
}