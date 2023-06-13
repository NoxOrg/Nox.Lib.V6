using System.Transactions;
using ETLBox.Connection;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.Data.MySqlClient;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Models;
using Nox.Solution;
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

    public override string ToDatabaseColumnType(NoxSimpleTypeDefinition entityAttribute)
    {
        //todo use NoxType underlying type to map to db types
        var propWidth = 0;
        var propPrecision = 0;
        var isFixedWidth = false;
        if (entityAttribute.TextTypeOptions != null)
        {
            propWidth = entityAttribute.TextTypeOptions.MaxLength < 1 ? 65534 : entityAttribute.TextTypeOptions.MaxLength;
            isFixedWidth = entityAttribute.TextTypeOptions.MinLength == entityAttribute.TextTypeOptions.MaxLength;
        }

        if (entityAttribute.NumberTypeOptions != null)
        {
            propWidth = entityAttribute.NumberTypeOptions.IntegerDigits + entityAttribute.NumberTypeOptions.DecimalDigits;
            propPrecision = entityAttribute.NumberTypeOptions.DecimalDigits;
        }
        

        //     "real" => typeof(Single),
        //     "float" => typeof(Single),
        //     "bigreal" => typeof(Double),
        //     "bigfloat" => typeof(Double),

        return entityAttribute.Type switch
        {
            //todo use NoxType underlying type to map to db types
            //
            // NoxType.text or NoxType.password => isFixedWidth ? $"char({propWidth})" : $"varchar({propWidth})",
            // NoxType.url => "varchar(2048)",
            // NoxType.email => "varchar(320)",
            // NoxType.guid => "binary(16)",
            // NoxType.date => "date",
            // NoxType.dateTime => "datetime",
            // NoxType.time or NoxType.dateTimeDuration => "timestamp",
            // NoxType.boolean => "tinyint(1)",
            // NoxType.@object => null!,
            

            // "varchar" => $"varchar({propWidth})",
            // "nvarchar" => $"varchar({propWidth})",
            // "url" => 
            // "email" => "varchar(320)",
            // "char" => $"char({propWidth})",
            // "guid" => "binary(16)",
            // "date" => "date",
            // "datetime" => "datetime",
            // "time" => "timestamp",
            // "timespan" => "timestamp",
            // "bool" => "tinyint(1)",
            // "boolean" => "tinyint(1)",
            // "object" => null!,
            // "int" => "integer",
            // "uint" => "integer",
            // "bigint" => "bigint",
            // "smallint" => "smallint",
            // "decimal" => $"decimal({propWidth},{propPrecision})",
            // "money" => $"decimal({propWidth},{propPrecision})",
            // "smallmoney" => $"decimal({propWidth},{propPrecision})",
            // _ => "varchar"
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