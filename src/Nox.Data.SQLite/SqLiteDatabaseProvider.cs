using ETLBox.Connection;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Models;
using Nox.Solution;
using SqlKata.Compilers;

namespace Nox.Data.SQLite;

public class SqLiteDatabaseProvider: DatabaseProviderBase
{
    public SqLiteDatabaseProvider()
    {
        _name = DataProvider.SqLite;

        _connectionManager = new SQLiteConnectionManager();

        _sqlCompiler = new SqlServerCompiler();
    }

    public override void Configure(IServiceDataSource serviceDb, string applicationName)
    {
        SqliteConnectionStringBuilder csb;

        csb = new SqliteConnectionStringBuilder(serviceDb.ConnectionString);

        serviceDb.ConnectionString = csb.ToString();

        SetConnectionString(serviceDb.ConnectionString);
    }

    protected override void SetConnectionString(string connectionString)
    {
        base.SetConnectionString(connectionString);
        _connectionManager.ConnectionString = new SQLiteConnectionString(connectionString);
    }

    public override DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseSqlite(_connectionString, x =>
            x.MigrationsHistoryTable("MigrationsHistory", "migrations")
        );
    }

    public override string ToDatabaseColumnType(NoxSimpleTypeDefinition entityAttribute)
    {
        //todo use NoxType underlying type to map to db types
        return "text";
        // var propType = entityAttribute.Type?.ToLower() ?? "string";
        //
        // //     "real" => typeof(Single),
        // //     "float" => typeof(Single),
        // //     "bigreal" => typeof(Double),
        // //     "bigfloat" => typeof(Double),
        //
        // return propType switch
        // {
        //     "bool" => "integer",
        //     "boolean" => "integer",
        //     "object" => null!,
        //     "int" => "integer",
        //     "uint" => "integer",
        //     "bigint" => "integer",
        //     "smallint" => "integer",
        //     "decimal" => "real",
        //     "money" => "real",
        //     "smallmoney" => "real",
        //     _ => "text"
        // };
    }

    public override EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        builder.ToTable(table, schema);
        return builder;
    }

    public override IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        configuration.UseMemoryStorage();
        return configuration;
    }

    public override string ToTableNameForSql(string table, string schema)
    {
        return $"[{table}]";
    }

    public override string ToTableNameForSqlRaw(string table, string schema)
    {
        return $"{table}";
    }

}