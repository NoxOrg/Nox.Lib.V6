using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.MetaData;
using Nox.Dynamic.Services;
using SqlKata;
using SqlKata.Compilers;
using System.Dynamic;

namespace Nox.Dynamic.Loaders;

internal class LoaderExecutor : ILoaderExecutor
{

    private readonly ILogger<LoaderExecutor> _logger;
    
    private readonly IBus _bus;
    
    public LoaderExecutor(ILogger<LoaderExecutor> logger, IBus bus)
    {
        _logger = logger;
        
        _bus = bus;       
    }

    public async Task<bool> ExecuteAsync(Service service)
    {
        // ETLBox.Logging.Logging.LogInstance = _logger;

        var destinationDbProvider = service.Database.DatabaseProvider!;

        var loaders = service.Loaders;

        var entities = service.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var loader in loaders)
        {
            await LoadDataFromSource(destinationDbProvider, loader, entities[loader.Target.Entity]);
        }

        return true;
    }

    public async Task<bool> ExecuteLoaderAsync(
        Loader loader, IDatabaseProvider destinationDbProvider, Entity entity)
    {
        // ETLBox.Logging.Logging.LogInstance = _logger;

        // Hack to accomodate Hangfire bug with collection initialization
        // (zero length List and Array comes back with single blank entry!

        entity.ApplyDefaults();
        entity.Attributes.ToList().ForEach(a => a.ApplyDefaults());

        await LoadDataFromSource(destinationDbProvider, loader, entity);

        return true;
    }

    private async Task LoadDataFromSource(IDatabaseProvider destinationDbProvider,
        Loader loader, Entity entity)
    {
        var destinationDb = destinationDbProvider.ConnectionManager;

        var destinationTable = destinationDbProvider.ToTableNameForSql(entity);

        var destinationSqlCompiler = destinationDbProvider.SqlCompiler;

        foreach (var loaderSource in loader.Sources)
        {
            if (loaderSource.DatabaseProvider is null)
            {
                continue;
            }

            var sourceDb = loaderSource.DatabaseProvider.ConnectionManager;


            var sourceSqlCompiler = loaderSource.DatabaseProvider.SqlCompiler!;

            var loadStrategy = loader.LoadStrategy.Type.Trim().ToLower();

            switch (loadStrategy)
            {
                case "dropandload":
                    _logger.LogInformation("Dropping and loading data for entity {entity}...", entity.Name);

                    await DropAndLoadData(sourceDb, destinationDb, loaderSource, destinationTable);

                    break;

                case "mergenew":
                    _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);

                    await MergeNewData(sourceDb, destinationDb,
                        loaderSource, loader, destinationTable, entity,
                        sourceSqlCompiler, destinationSqlCompiler);

                    break;

                default:

                    _logger.LogError("{message}",$"Unsupported load strategy '{loader.LoadStrategy.Type}' in loader '{loader.Name}'.");

                    break;

            };

        }
    }

    private async Task DropAndLoadData(
        IConnectionManager sourceDb,
        IConnectionManager destinationDb,
        LoaderSource loaderSource, string destinationTable)
    {
        var source = new DbSource()
        {
            ConnectionManager = sourceDb,
            Sql = loaderSource.Query,
        };

        var destination = new DbDestination()
        {
            ConnectionManager = destinationDb,
            TableName = destinationTable,
        };

        source.LinkTo(destination);

        SqlTask.ExecuteNonQuery(destinationDb, $"DELETE FROM {destinationTable};");

        await Network.ExecuteAsync(source);

        int rowCount = RowCountTask.Count(destinationDb, destinationTable);

        _logger.LogInformation("...copied {rowCount} records", rowCount);
    }


    private async Task<bool> MergeNewData(
        IConnectionManager sourceDb,
        IConnectionManager destinationDb,
        LoaderSource loaderSource,
        Loader loader,
        string destinationTable,
        Entity entity,
        Compiler sourceSqlCompiler,
        Compiler destinationSqlCompiler)
    {
        var newLastMergeDateTimeStamp = new Dictionary<string, (DateTime LastMergeDateTimeStamp, bool Updated)>();

        var targetColumns = entity.Attributes.Where(a => a.IsMappedAttribute()).Select(a => a.Name)
                .Concat(entity.RelatedParents.Select(p => p + "Id"))
                .Concat(loader.LoadStrategy.Columns.Select(c => c))
                .ToArray();

        var primaryColumns = entity.Attributes.Where(a => a.IsPrimaryKey && a.IsMappedAttribute()).Select(a => a.Name)
                .Concat(entity.RelatedParents.Select(p => p + "Id"))
                .ToArray();

        var compairColumns = entity.Attributes.Where(a => !a.IsPrimaryKey && !loader.LoadStrategy.Columns.Contains(a.Name) && a.IsMappedAttribute()).Select(a => a.Name)
                .Concat(entity.RelatedParents.Select(p => p + "Id"))
                .ToArray();

        var query = new Query().FromRaw($"({loaderSource.Query}) AS tmp")
            .Select(targetColumns);

        var lastMergeDateTimeStamp = DateTime.MinValue;

        foreach (var dateColumn in loader.LoadStrategy.Columns)
        {
            lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, destinationSqlCompiler);

            if (!lastMergeDateTimeStamp.Equals(DateTime.MinValue))
            {
                query = query.Where(
                    q => q.WhereNotNull(dateColumn).Where(dateColumn, ">", lastMergeDateTimeStamp)
                );
            }

            newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp = lastMergeDateTimeStamp, Updated = false };
        }

        var compiledQuery = sourceSqlCompiler.Compile(query);

        var finalQuerySql = compiledQuery.Sql;

        var finalQueryParams = compiledQuery.NamedBindings.Select(nb => new QueryParameter(nb.Key, nb.Value));

        var source = new DbSource()
        {
            ConnectionManager = sourceDb,
            Sql = finalQuerySql,
            SqlParameter = finalQueryParams,
        };

        var destination = new DbMerge(destinationDb, destinationTable)
        {
            CacheMode = ETLBox.DataFlow.Transformations.CacheMode.Partial,
            MergeMode = MergeMode.InsertsAndUpdates,
        };

        destination.MergeProperties.IdColumns =
            primaryColumns
            .Select(colName => new IdColumn() { IdPropertyName = colName })
            .ToArray();

     
        destination.MergeProperties.CompareColumns =
            compairColumns
            .Select(colName => new CompareColumn() { ComparePropertyName = colName })
            .ToArray();

        source.LinkTo(destination);

        var analatics = new CustomDestination();

        int inserts = 0;
        int updates = 0;
        int nochanges = 0;

        analatics.WriteAction = (row, _) =>
        {
            dynamic r = row;

            IDictionary<string, object?> d = new Dictionary<string, object?>(row);

            if (r.ChangeAction == ChangeAction.Insert)
            {
                inserts++;

                _bus.Publish(new LoaderInsertMessage { Value = d }).GetAwaiter().GetResult();

                foreach (var (dateColumn, (timeStamp, updated)) in newLastMergeDateTimeStamp)
                {

                    if (d[dateColumn] != null)
                    {
                        var fieldValue = (DateTime)d[dateColumn]!;
                        if (fieldValue > newLastMergeDateTimeStamp[dateColumn].LastMergeDateTimeStamp)
                        {
                            var changeEntry = newLastMergeDateTimeStamp[dateColumn];
                            changeEntry.LastMergeDateTimeStamp = fieldValue;
                            changeEntry.Updated = true;
                            newLastMergeDateTimeStamp[dateColumn] = changeEntry;
                        }
                    }
                }

            }
            else if (r.ChangeAction == ChangeAction.Update)
            {
                updates++;

                _bus.Publish(new LoaderUpdateMessage() { Value = d }).GetAwaiter().GetResult();

                foreach (var (dateColumn, (timeStamp, updated)) in newLastMergeDateTimeStamp)
                {

                    if (d[dateColumn] != null)
                    {
                        var fieldValue = (DateTime)d[dateColumn]!;
                        if (fieldValue > newLastMergeDateTimeStamp[dateColumn].LastMergeDateTimeStamp)
                        {
                            var changeEntry = newLastMergeDateTimeStamp[dateColumn];
                            changeEntry.LastMergeDateTimeStamp = fieldValue;
                            changeEntry.Updated = true;
                            newLastMergeDateTimeStamp[dateColumn] = changeEntry;
                        }
                    }
                }
            }
            else if (r.ChangeAction == ChangeAction.Exists)
            {
                nochanges++;
            }
        };

        destination.LinkTo(analatics);

        try
        {
            Network.Execute(source);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Failed to run Merge for Entity {entity} at {lastMergeDateTimeStamp}", entity.Name, lastMergeDateTimeStamp);
            _logger.LogError("{message}", ex.Message);
        }

        if (inserts == 0 && updates == 0)
        {
            if (nochanges > 0)
            {
                _logger.LogInformation(
                    "{nochanges} records found but no change found to merge, last merge at: {lastMergeDateTimeStamp}", nochanges, lastMergeDateTimeStamp);
            }
            else
            {
                _logger.LogInformation("...no changes found to merge");
            }
            return false;
        }

        _logger.LogInformation("{inserts} records inserted, last merge at {lastMergeDateTimeStamp}", inserts, lastMergeDateTimeStamp);
        _logger.LogInformation("{updates} records updated, last merge at {lastMergeDateTimeStamp}", updates, lastMergeDateTimeStamp);

        foreach (var (dateColumn, (timeStamp, updated)) in newLastMergeDateTimeStamp)
        {
            if (updated)
            {
                SetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, timeStamp, destinationSqlCompiler);
            }
        }
        return true;

    }

    private static DateTime GetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName, string dateColumn, Compiler compiler)
    {
        var lastMergeDateTime = DateTime.MinValue;

        var destination = new DbDestination()
        {
            ConnectionManager = destinationDb,
            TableName = Constants.Database.MergeStateTable,
        };

        var findQuery = new Query(
                $"meta.{Constants.Database.MergeStateTable}")
                .Where("Property", dateColumn)
                .Where("Loader", loaderName)
                .Select("LastDateLoadedUtc");

        var findSql = compiler.Compile(findQuery).ToString();

        object? resultDate = null;
        SqlTask.ExecuteReader(destinationDb, findSql, r => resultDate = r);
        if (resultDate is not null)
        {
            return (DateTime)resultDate;
        }

        var insertQuery = new Query($"meta.{Constants.Database.MergeStateTable}").AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoadedUtc = lastMergeDateTime
        });

        var insertSql = compiler.Compile(insertQuery).ToString();

        SqlTask.ExecuteNonQuery(destinationDb, insertSql);

        return lastMergeDateTime;

    }

    private bool SetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName,
        string dateColumn, DateTime lastMergeDateTime, Compiler compiler)
    {

        _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

        var updateQuery = new Query($"meta.{Constants.Database.MergeStateTable}")
          .Where("Property", dateColumn)
          .Where("Loader", loaderName)
          .AsUpdate(
          new
          {
              LastDateLoadedUtc = lastMergeDateTime
          });

        var updateSql = compiler.Compile(updateQuery).ToString();

        var result = SqlTask.ExecuteNonQuery(destinationDb, updateSql);

        return result == 1;
    }

}
