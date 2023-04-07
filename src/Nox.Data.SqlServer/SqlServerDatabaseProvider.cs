using ETLBox.Connection;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Models;
using SqlKata.Compilers;
using System.Data.SqlClient;

namespace Nox.Data.SqlServer;

public class SqlServerDatabaseProvider: DatabaseProviderBase
{
    public SqlServerDatabaseProvider()
    {
        _name = DataProvider.SqlServer;

        _connectionManager = new SqlConnectionManager();

        _sqlCompiler = new SqlServerCompiler();
    }

    public override void Configure(IServiceDataSource serviceDb, string applicationName)
    {
        SqlConnectionStringBuilder csb;

        if (string.IsNullOrEmpty(serviceDb.ConnectionString))
        {
            csb = new SqlConnectionStringBuilder(serviceDb.Options)
            {
                DataSource = $"{serviceDb.Server},{serviceDb.Port}",
                UserID = serviceDb.User,
                Password = serviceDb.Password,
                InitialCatalog = serviceDb.Name,
            };
        }
        else
        {
            csb = new SqlConnectionStringBuilder(serviceDb.ConnectionString);
        }

        csb.ApplicationName = applicationName;

        serviceDb.ConnectionString = csb.ToString();

        SetConnectionString(serviceDb.ConnectionString);
    }

    protected override void SetConnectionString(string connectionString)
    {
        base.SetConnectionString(connectionString);

        _connectionManager.ConnectionString = new SqlConnectionString(_connectionString);
    }

    public override DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseSqlServer(_connectionString, x =>
            x.MigrationsHistoryTable("MigrationsHistory", "migrations")
        );
    }

    public override string ToDatabaseColumnType(BaseEntityAttribute entityAttribute)
    {
        var propType = entityAttribute.Type?.ToLower() ?? "string";
        var propWidth = entityAttribute.MaxWidth < 1 ? "max" : entityAttribute.MaxWidth.ToString();
        var propPrecision = entityAttribute.Precision.ToString();

        //     "real" => typeof(Single),
        //     "float" => typeof(Single),
        //     "bigreal" => typeof(Double),
        //     "bigfloat" => typeof(Double),

        return propType switch
        {
            "string" => entityAttribute.IsUnicode ? $"nvarchar({propWidth})" : $"varchar({propWidth})",
            "varchar" => $"varchar({propWidth})",
            "nvarchar" => $"nvarchar({propWidth})",
            "url" => "varchar(2048)",
            "email" => "varchar(320)",
            "char" => entityAttribute.IsUnicode ? $"nchar({propWidth})" : $"char({propWidth})",
            "guid" => "uniqueidentifier",
            "date" => "date",
            "datetime" => "datetimeoffset",
            "time" => "datetime",
            "timespan" => "timespan",
            "bool" => "bit",
            "boolean" => "bit",
            "object" => "sql_variant",
            "int" => "int",
            "uint" => "uint",
            "bigint" => "bigint",
            "smallint" => "smallint",
            "decimal" => $"decimal({propWidth},{propPrecision})",
            "money" => $"decimal({propWidth},{propPrecision})",
            "smallmoney" => $"decimal({propWidth},{propPrecision})",
            _ => "nvarchar(max)"
        };
    }

    public override EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        builder.ToTable(table, schema);
        return builder;
    }

    public override IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        configuration.UseSqlServerStorage(_connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "jobs",
            PrepareSchemaIfNecessary = true,
        });

        return configuration;
    }

    public override string ToTableNameForSql(string table, string schema)
    {
        return $"[{schema}].[{table}]";
    }

    public override string ToTableNameForSqlRaw(string table, string schema)
    {
        return $"{schema}.{table}";
    }
}