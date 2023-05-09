using System.Transactions;
using ETLBox.Connection;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.Data.MySqlClient;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using SqlKata.Compilers;

namespace Nox.Data.MySql;

public class MySqlDatabaseProvider: DatabaseProviderBase
{

    public MySqlDatabaseProvider()
    {
        _name = DataProvider.MySql;

        _connectionManager = new MySqlConnectionManager();

        _sqlCompiler = new MySqlCompiler();
    }

    public override void Configure(IServiceDataSource serviceDb, string applicationName)
    {
        MySqlConnectionStringBuilder csb;

        if (string.IsNullOrEmpty(serviceDb.ConnectionString))
        {
            csb = new MySqlConnectionStringBuilder(serviceDb.Options)
            {
                Server = serviceDb.Server,
                Port = (uint)serviceDb.Port,
                UserID = serviceDb.User,
                Password = serviceDb.Password,
                Database = serviceDb.Name
            };
        }
        else
        {
            csb = new MySqlConnectionStringBuilder(serviceDb.ConnectionString);
        }

        serviceDb.ConnectionString = csb.ToString();

        SetConnectionString(serviceDb.ConnectionString);
    }
    
    protected override void SetConnectionString(string connectionString) 
    {
        base.SetConnectionString(connectionString);

        _connectionManager.ConnectionString = new MySqlConnectionString(connectionString);
    }

    public override DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseMySQL(_connectionString, x =>
            x.MigrationsHistoryTable("migrations_MigrationsHistory")
        );
    }

    public override string ToDatabaseColumnType(IEntityAttribute entityAttribute)
    {
        var propType = entityAttribute.Type?.ToLower() ?? "string";
        var propWidth = entityAttribute.MaxWidth < 1 ? "65535" : entityAttribute.MaxWidth.ToString();
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
            "guid" => "binary(16)",
            "date" => "date",
            "datetime" => "datetime",
            "time" => "timestamp",
            "timespan" => "timestamp",
            "bool" => "tinyint(1)",
            "boolean" => "tinyint(1)",
            "object" => null!,
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

    public override EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        builder.ToTable($"{schema}_{table}");
        return builder;
    }

    public override IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        configuration.UseStorage(
            new MySqlStorage(_connectionString, new MySqlStorageOptions
            {
                TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "jobs_",
            }));
        return configuration;
    }

    public override string ToTableNameForSql(string table, string schema)
    {
        return $"`{schema}_{table}`";
    }

    public override string ToTableNameForSqlRaw(string table, string schema)
    {
        return $"{schema}_{table}";
    }

}