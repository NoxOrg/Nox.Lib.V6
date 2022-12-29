using System.Dynamic;
using ETLBox.Connection;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Constants;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Models;
using SqlKata.Compilers;

namespace Nox.Data.Parquet;

public class ParquetDataProvider : IDataProvider
{

    protected ParquetSource<ExpandoObject> _dataFlowExecutableSource = null!;

    public string Name => DataProvider.Parquet;

    private string _options = @"Path=.\";

    private string _folderPath = string.Empty;

    private ResourceType _resourceType = ResourceType.File;

    public string ConnectionString
    {
        get { return _options; }

        set { SetConnectionString(value); }
    }

    protected virtual void SetConnectionString(string connectionString)
    {
        _options = connectionString;

        var options = _options.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Split('=', 2))
                .ToDictionary(e => e[0], e => e[1], StringComparer.OrdinalIgnoreCase);

        if (!options.ContainsKey("path"))
        {
            throw new Exception("'Path' is a required setting for Parquet data provider in 'Options'");
        }
        _folderPath = Path.GetFullPath(options["path"]);

        if (options.ContainsKey("source"))
        {
            _resourceType = options["source"].ToLower() switch
            {
                "http" => ResourceType.Http,
                "https" => ResourceType.Http,
                "blob" => ResourceType.AzureBlob,
                "azureblob" => ResourceType.AzureBlob,
                "file" => ResourceType.File,
                "filesystem" => ResourceType.File,
                _ => throw new Exception($"Invalid Parquet source '{options["source"]}' specified in 'Options'")
            };
        }
        else
        {
            _resourceType= ResourceType.File;
        }

    }

    public Compiler SqlCompiler => null!;

    public IConnectionManager ConnectionManager => null!;

    public void Configure(IServiceDataSource serviceDb, string applicationName)
    {

        _options = serviceDb.Options;

        SetConnectionString(_options);
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
        _dataFlowExecutableSource = new ParquetSource<ExpandoObject>();

        switch (_resourceType)
        {
            case ResourceType.File:
                var filePath = Path.Combine(_folderPath, loaderSource.Query);
                _dataFlowExecutableSource.ResourceType = ResourceType.File;
                _dataFlowExecutableSource.Uri = filePath;
                break;

            case ResourceType.Http:
                var baseUri = new Uri(_folderPath);
                var uri = new Uri(baseUri, loaderSource.Query);
                _dataFlowExecutableSource.ResourceType = ResourceType.Http;
                _dataFlowExecutableSource.Uri = uri.ToString();
                break;

            case ResourceType.AzureBlob:
                throw new NotImplementedException();
                // break;

            default:
                break;
        }

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