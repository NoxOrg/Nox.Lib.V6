using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces;
using Nox.Core.Models;
using Nox.Messaging;
using Nox.Messaging.Enumerations;
using Nox.Messaging.Events;
using SqlKata;
using SqlKata.Compilers;

namespace Nox.Etl;

public class EtlExecutor : IEtlExecutor
{
    private readonly ILogger<EtlExecutor> _logger;
    private readonly IEnumerable<INoxEvent> _messages;
    private readonly INoxMessenger? _messenger;
    
    public EtlExecutor(
        ILogger<EtlExecutor> logger,
        IEnumerable<INoxEvent> messages,
        INoxMessenger? messenger = null)
    {
        _logger = logger;
        _messages = messages;
        _messenger = messenger;
    }

    public async Task<bool> ExecuteAsync(IMetaService service)
    {
        // ETLBox.Logging.Logging.LogInstance = _logger;

        var loaders = service.Loaders;

        var entities = service.Entities!.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var loader in loaders!)
        {
            await LoadDataFromSource(service, loader, entities[loader.Target!.Entity]);
        }

        return true;
    }

    public async Task<bool> ExecuteLoaderAsync(IMetaService service, ILoader loader, IEntity entity)
    {

        entity.ApplyDefaults();
        entity.Attributes.ToList().ForEach(a => a.ApplyDefaults());

        await LoadDataFromSource(service, loader, entity);

        return true;
    }

    private async Task LoadDataFromSource(IMetaService service, ILoader loader, IEntity entity)
    {
        var destinationDb = service.Database!.DatabaseProvider!.ConnectionManager;

        var destinationTable = service.Database!.DatabaseProvider!.ToTableNameForSql(entity.Table, entity.Schema);

        var destinationSqlCompiler = service.Database!.DatabaseProvider!.SqlCompiler;

        var loaderInstance = (Loader)loader;
        foreach (var loaderSource in loaderInstance.Sources!)
        {
            var dataSource = service.DataSources!.First(ds => ds.Name == loaderSource.Name);
            
            var sourceDb = dataSource.DatabaseProvider!.ConnectionManager;

            var sourceSqlCompiler = dataSource.DatabaseProvider.SqlCompiler;

            var loadStrategy = loaderInstance.LoadStrategy?.Type.Trim().ToLower();

            switch (loadStrategy)
            {
                case "dropandload":
                    _logger.LogInformation("Dropping and loading data for entity {entity}...", entity.Name);

                    await DropAndLoadData(sourceDb, destinationDb, loaderSource, destinationTable);

                    break;

                case "mergenew":
                    _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);

                    await MergeNewData(sourceDb, destinationDb,
                        loaderSource, service, loaderInstance, destinationTable, entity,
                        sourceSqlCompiler, destinationSqlCompiler);

                    break;

                default:

                    _logger.LogError("{message}",$"Unsupported load strategy '{loaderInstance.LoadStrategy!.Type}' in loader '{loaderInstance.Name}'.");

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


    private async Task MergeNewData(IConnectionManager sourceDb,
        IConnectionManager destinationDb,
        ILoaderSource loaderSource,
        IMetaService service,
        Loader loader,
        string destinationTable,
        IEntity entity,
        Compiler sourceSqlCompiler,
        Compiler destinationSqlCompiler)
    {
        var newLastMergeDateTimeStamp = new Dictionary<string, (DateTime LastMergeDateTimeStamp, bool Updated)>();

        var targetColumns = entity.Attributes.Where(a => a.IsMappedAttribute()).Select(a => a.Name)
                .Concat(entity.RelatedParents.Select(p => p + "Id"))
                .Concat(loader.LoadStrategy!.Columns.Select(c => c))
                .ToArray();

        var query = new Query().FromRaw($"({loaderSource.Query}) AS tmp")
            .Select(targetColumns);

        var lastMergeDateTimeStamp = DateTime.MinValue;

        foreach (var dateColumn in loader.LoadStrategy.Columns)
        {
            lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, destinationSqlCompiler, service.Database!.DatabaseProvider!);

            if (!lastMergeDateTimeStamp.Equals(DateTime.MinValue))
            {
                var stamp = lastMergeDateTimeStamp;
                query = query.Where(
                    q => q.WhereNotNull(dateColumn).Where(dateColumn, ">", stamp)
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
            BatchSize = 1,
        };

        destination.MergeProperties.IdColumns =
            targetColumns
            .Skip(0)
            .Take(1)
            .Select(colName => new IdColumn() { IdPropertyName = colName })
            .ToArray();

        destination.MergeProperties.CompareColumns =
            targetColumns
            .Skip(1)
            .Take(targetColumns.Length - 1 - loader.LoadStrategy.Columns.Length)
            .Select(colName => new CompareColumn() { ComparePropertyName = colName })
            .ToArray();

        source.LinkTo(destination);

        var analytics = new CustomDestination();

        int inserts = 0;
        int updates = 0;
        int nochanges = 0;

        analytics.WriteAction = (row, _) =>
        {
            dynamic r = row;

            IDictionary<string, object?> d = new Dictionary<string, object?>(row);

            if (r.ChangeAction == ChangeAction.Insert)
            {
                inserts++;
                var msg = _messages.FindEventImplementation(entity, NoxEventTypeEnum.Create);
                
                if (loader.Messaging != null && loader.Messaging.Any() && msg != null)
                {
                    var toSend = msg.MapInstance(row);
                    _messenger?.SendMessage(loader, toSend);
                }
                
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
                
                var msg = _messages.FindEventImplementation(entity, NoxEventTypeEnum.Update);
                
                if (loader.Messaging != null && loader.Messaging.Any() && msg != null)
                {
                    var toSend = msg.MapInstance(row);
                    _logger.LogInformation($"Publishing bus message: {toSend.GetType().Name}");
                    _messenger?.SendMessage(loader, toSend);
                }
                
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

        destination.LinkTo(analytics);

        try
        {
            await Network.ExecuteAsync(source);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Failed to run Merge for Entity {entity} at {lastMergeDateTimeStamp}", entity.Name, lastMergeDateTimeStamp);
            _logger.LogError("{message}", ex.Message);
            throw;
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

            return;
        }

        _logger.LogInformation("{inserts} records inserted, last merge at {lastMergeDateTimeStamp}", inserts, lastMergeDateTimeStamp);
        _logger.LogInformation("{updates} records updated, last merge at {lastMergeDateTimeStamp}", updates, lastMergeDateTimeStamp);

        foreach (var (dateColumn, (timeStamp, updated)) in newLastMergeDateTimeStamp)
        {
            if (updated)
            {
                SetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, timeStamp, destinationSqlCompiler, service.Database!.DatabaseProvider!);
            }
        }
    }

    private static DateTime GetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName, string dateColumn, 
        Compiler destinationSqlCompiler, IDatabaseProvider destinationDbProvider)
    {
        var lastMergeDateTime = DateTime.MinValue;

        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);

        var findQuery = new Query(mergeStateTableName)
                .Where("Property", dateColumn)
                .Where("Loader", loaderName)
                .Select("LastDateLoadedUtc");

        var findSql = destinationSqlCompiler.Compile(findQuery).ToString();

        object? resultDate = null;
        SqlTask.ExecuteReader(destinationDb, findSql, r => resultDate = r);
        if (resultDate is not null)
        {
            return (DateTime)resultDate;
        }

        var insertQuery = new Query(mergeStateTableName).AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoadedUtc = lastMergeDateTime
        });

        var insertSql = destinationSqlCompiler.Compile(insertQuery).ToString();

        SqlTask.ExecuteNonQuery(destinationDb, insertSql);

        return lastMergeDateTime;

    }

    private bool SetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName,
        string dateColumn, DateTime lastMergeDateTime, Compiler destinationSqlCompiler,
        IDatabaseProvider destinationDbProvider)
    {
        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);

        _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

        var updateQuery = new Query(mergeStateTableName)
          .Where("Property", dateColumn)
          .Where("Loader", loaderName)
          .AsUpdate(
          new
          {
              LastDateLoadedUtc = lastMergeDateTime
          });

        var updateSql = destinationSqlCompiler.Compile(updateQuery).ToString();

        var result = SqlTask.ExecuteNonQuery(destinationDb, updateSql);

        return result == 1;
    }
}