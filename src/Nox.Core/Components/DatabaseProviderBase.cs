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
using Nox.Core.Constants;

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

    public virtual EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        throw new NotImplementedException();
    }

    public IDataFlowExecutableSource<ExpandoObject> DataFlowSource(ILoaderSource loaderSource)
    {
        _dataFlowExecutableSource.ConnectionManager = _connectionManager;
        _dataFlowExecutableSource.Sql = loaderSource.Query;
        return _dataFlowExecutableSource;
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
            if (lastMergeDateTimeStampInfo.Any(c => c.Key.Equals(dateColumn, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (!lastMergeDateTimeStampInfo[dateColumn].LastDateLoadedUtc.Equals(NoxDateTime.MinSqlDate))
                { 
                    query = query.OrWhere(
                        q => q.WhereNotNull(dateColumn).Where(dateColumn, ">", lastMergeDateTimeStampInfo[dateColumn].LastDateLoadedUtc)
                    );
                }    
            }
            else
            {
                if (lastMergeDateTimeStampInfo.Any(c => c.Key.Equals(EtlExecutorConstants.DefaultMergeProperty, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!lastMergeDateTimeStampInfo[EtlExecutorConstants.DefaultMergeProperty].LastDateLoadedUtc.Equals(NoxDateTime.MinSqlDate))
                    { 
                        query = query.OrWhere(
                            q => q.WhereNotNull(dateColumn).Where(dateColumn, ">", lastMergeDateTimeStampInfo[EtlExecutorConstants.DefaultMergeProperty].LastDateLoadedUtc)
                        );
                    }    
                }
            }
        }

        var compiledQuery = _sqlCompiler.Compile(query);

         _dataFlowExecutableSource.Sql = compiledQuery.Sql;
        
         _dataFlowExecutableSource.SqlParameter = compiledQuery.NamedBindings.Select(nb => new QueryParameter(nb.Key, nb.Value));

    }

    public virtual void Configure(IServiceDataSource serviceDb, string applicationName)
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
