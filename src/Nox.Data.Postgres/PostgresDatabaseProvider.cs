using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.DataFlow;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Npgsql;
using SqlKata.Compilers;
using System.Dynamic;

namespace Nox.Data.Postgres;

public class PostgresDatabaseProvider: IDataProvider
{
    private string _connectionString = string.Empty;

    private readonly IConnectionManager _connectionManager = new PostgresConnectionManager();

    private readonly Compiler _sqlCompiler = new PostgresCompiler();

    public IConnectionManager ConnectionManager => _connectionManager;
    public Compiler SqlCompiler => _sqlCompiler;
    public string ConnectionString
    {
        get { return _connectionString; }

        set { SetConnectionString(value); }
    }

    public string Name => "postgres";

    public void ConfigureServiceDatabase(IServiceDataSource serviceDb, string applicationName)
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

    private void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;

        _connectionManager.ConnectionString = new PostgresConnectionString(connectionString);
    }

    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseNpgsql(_connectionString);
    }

    public string ToDatabaseColumnType(IEntityAttribute entityAttribute)
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

    public IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        configuration.UsePostgreSqlStorage(_connectionString, new PostgreSqlStorageOptions
        {
            SchemaName = "jobs",
            PrepareSchemaIfNecessary = true,
        });
        return configuration;
    }

    public string ToTableNameForSql(string table, string schema)
    {
        return $"\"{schema}\".\"{table}\"";
    }

    public string ToTableNameForSqlRaw(string table, string schema)
    {
        return $"{schema}.{table}";
    }

    public EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        builder.ToTable(table, schema);
        return builder;
    }

    public IDataFlowExecutableSource<ExpandoObject> DataFlowSource(ILoaderSource loaderSource)
    {
        return new DbSource<ExpandoObject>()
        {
            ConnectionManager = _connectionManager,
            Sql = loaderSource.Query
        };
    }

}