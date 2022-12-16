using ETLBox.Connection;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Npgsql;
using SqlKata.Compilers;
using Nox.Core.Components;

namespace Nox.Data.Postgres;

public class PostgresDatabaseProvider: DatabaseProviderBase
{
    public PostgresDatabaseProvider()
    {
        _name = "postgres";

        _connectionManager = new PostgresConnectionManager();

        _sqlCompiler = new PostgresCompiler();

    }


    public override void ConfigureServiceDatabase(IServiceDataSource serviceDb, string applicationName)
    {
        NpgsqlConnectionStringBuilder csb;

        if (string.IsNullOrEmpty(serviceDb.ConnectionString))
        {
            csb = new NpgsqlConnectionStringBuilder(serviceDb.Options)
            {
                Host = serviceDb.Server,
                Port = serviceDb.Port,
                Username = serviceDb.User,
                Password = serviceDb.Password,
                Database = serviceDb.Name,
            };
        }
        else
        {
            csb = new NpgsqlConnectionStringBuilder(serviceDb.ConnectionString);
        }

        csb.ApplicationName = applicationName;

        serviceDb.ConnectionString = csb.ToString();

        SetConnectionString(serviceDb.ConnectionString);

    }

    protected override void SetConnectionString(string connectionString) 
    {
        base.SetConnectionString(connectionString);

        _connectionManager.ConnectionString = new PostgresConnectionString(connectionString);
    }

    public override DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseNpgsql(_connectionString);
    }

    public override string ToDatabaseColumnType(IEntityAttribute entityAttribute)
    {
        var propType = entityAttribute.Type?.ToLower() ?? "string";
        var propWidth = entityAttribute.MaxWidth < 1 ? "2048" : entityAttribute.MaxWidth.ToString();
        var propPrecision = entityAttribute.Precision.ToString();
        var isFixedWidth = entityAttribute.MaxWidth == entityAttribute.MinWidth;

        //     "real" => typeof(Single),
        //     "float" => typeof(Single),
        //     "bigreal" => typeof(Double),
        //     "bigfloat" => typeof(Double),

        return propType switch
        {
            "string" => isFixedWidth ? $"char({propWidth})" : $"varchar({propWidth})",
            "varchar" => $"varchar({propWidth})",
            "nvarchar" => $"varchar({propWidth})",
            "url" => "varchar(2048)",
            "email" => "varchar(320)",
            "char" => $"char({propWidth})",
            "guid" => "uuid",
            "date" => "date",
            "datetime" => "timestamp with time zone",
            "time" => "timestamp without time zone",
            "timespan" => "timestamp without time zone",
            "bool" => "boolean",
            "boolean" => "boolean",
            "object" => "jsonb",
            "int" => "integer",
            "uint" => "integer",
            "bigint" => "bigint",
            "smallint" => "smallint",
            "decimal" => $"decimal({propWidth},{propPrecision})",
            "money" => $"decimal({propWidth},{propPrecision})",
            "smallmoney" => $"decimal({propWidth},{propPrecision})",
            _ => "varchar"
        };
    }

    public override IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        configuration.UsePostgreSqlStorage(_connectionString, new PostgreSqlStorageOptions
        {
            SchemaName = "jobs",
            PrepareSchemaIfNecessary = true,
        });
        return configuration;
    }

    public override string ToTableNameForSql(string table, string schema)
    {
        return $"\"{schema}\".\"{table}\"";
    }

    public override string ToTableNameForSqlRaw(string table, string schema)
    {
        return $"{schema}.{table}";
    }


}