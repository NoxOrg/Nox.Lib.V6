using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.DataFlow;
using ETLBox.ControlFlow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Interfaces.Etl;
using SqlKata.Compilers;
using System.Dynamic;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Models;
using SqlKata;
using Nox.Core.Interfaces.Database;
using Hangfire;

namespace Nox.Core.Components;

public abstract class DatabaseProviderBase : IDataProvider
{
    protected string _name = string.Empty;
    public string Name => _name;


    protected string _connectionString = string.Empty;
    public string ConnectionString
    {
        get { return _connectionString; }

        set { SetConnectionString(value); }
    }
    protected virtual void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected IConnectionManager _connectionManager = null!;
    public IConnectionManager ConnectionManager => _connectionManager;

    protected Compiler _sqlCompiler = null!;
    public Compiler SqlCompiler => _sqlCompiler;

    protected readonly DbSource<ExpandoObject> _dataFlowExecutableSource = new();

    public EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        builder.ToTable($"{schema}_{table}");
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

    public void ApplyMergeInfo(
        ILoaderSource loaderSource,
        LoaderMergeStates lastMergeDateTimeStampInfo,
        string[] dateTimeStampColumns,
        string[] targetColumns)
    {

        var query = new Query().FromRaw($"({loaderSource.Query}) AS tmp")
            .Select(targetColumns);

        foreach (var dateColumn in dateTimeStampColumns)
        {
            if (!lastMergeDateTimeStampInfo[dateColumn].Equals(DateTime.MinValue))
            {
                query = query.Where(
                    q => q.WhereNotNull(dateColumn).Where(dateColumn, ">", lastMergeDateTimeStampInfo[dateColumn])
                );
            }
        }

        var compiledQuery = _sqlCompiler.Compile(query);

        _dataFlowExecutableSource.Sql = compiledQuery.Sql;

        _dataFlowExecutableSource.SqlParameter = compiledQuery.NamedBindings.Select(nb => new QueryParameter(nb.Key, nb.Value));

    }

    public virtual void ConfigureServiceDatabase(IServiceDataSource serviceDb, string applicationName)
    {
        throw new NotImplementedException();
    }

    public virtual DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        throw new NotImplementedException();
    }

    public virtual IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public virtual string ToDatabaseColumnType(IEntityAttribute entityAttribute)
    {
        throw new NotImplementedException();
    }

    public virtual string ToTableNameForSql(string table, string schema)
    {
        throw new NotImplementedException();
    }

    public virtual string ToTableNameForSqlRaw(string table, string schema)
    {
        throw new NotImplementedException();
    }
}
