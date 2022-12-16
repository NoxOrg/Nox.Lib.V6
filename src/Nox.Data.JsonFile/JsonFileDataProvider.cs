using ETLBox.Connection;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Models;
using SqlKata.Compilers;
using System.Dynamic;

namespace Nox.Data.JsonFile;

public class JsonFileDataProvider : IDataProvider
{

    protected readonly JsonSource<ExpandoObject> _dataFlowExecutableSource = new();

    public string Name => "jsonfile";

    private string _folderPath = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;

    public Compiler SqlCompiler => throw new NotImplementedException();

    public IConnectionManager ConnectionManager => throw new NotImplementedException();

    public void Configure(IServiceDataSource serviceDb, string applicationName)
    {
        var options = serviceDb.Options.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Split('=',2))
            .ToDictionary(e => e[0], e => e[1], StringComparer.OrdinalIgnoreCase);

        if (!options.ContainsKey("path"))
        {
            throw new ArgumentNullException("Path");
        }

        _folderPath = Path.GetFullPath(options["path"]);

    }

    public void ApplyMergeInfo(ILoaderSource loaderSource, LoaderMergeStates lastMergeDateTimeStampInfo, string[] dateTimeStampColumns, string[] targetColumns)
    {
        // noop for now...
    }

    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        throw new NotImplementedException();
    }

    public EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
    {
        throw new NotImplementedException();
    }

    public IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public IDataFlowExecutableSource<ExpandoObject> DataFlowSource(ILoaderSource loaderSource)
    {
        var filePath = Path.Combine(_folderPath, loaderSource.Query);

        _dataFlowExecutableSource.ResourceType = ResourceType.File;

        _dataFlowExecutableSource.Uri = filePath;

        return _dataFlowExecutableSource;
    }

    public string ToDatabaseColumnType(IEntityAttribute entityAttribute)
    {
        throw new NotImplementedException();
    }

    public string ToTableNameForSql(string table, string schema)
    {
        throw new NotImplementedException();
    }

    public string ToTableNameForSqlRaw(string table, string schema)
    {
        throw new NotImplementedException();
    }
}