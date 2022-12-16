using ETLBox.Connection;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Models;
using Nox.Messaging;
using Nox.Messaging.Enumerations;
using Nox.Messaging.Events;
using System.Dynamic;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        
        var loaderInstance = (Loader)loader;

        var targetProvider = service.Database!.DataProvider!;

        var destinationDb = targetProvider.ConnectionManager;

        var destinationTable = targetProvider.ToTableNameForSql(entity.Table, entity.Schema);

        var destinationSqlCompiler = targetProvider.SqlCompiler;

        foreach (var loaderSource in loaderInstance.Sources!)
        {
            var sourceProvider = service.DataSources!.First(ds => ds.Name == loaderSource.DataSource).DataProvider!;

            var source = sourceProvider!.DataFlowSource(loaderSource);

            var loadStrategy = loaderInstance.LoadStrategy?.Type.Trim().ToLower();

            switch (loadStrategy)
            {
                case "dropandload":
                    _logger.LogInformation("Dropping and loading data for entity {entity}...", entity.Name);

                    await DropAndLoadData(source, destinationDb, destinationTable);

                    break;

                case "mergenew":
                    _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);

                    var lastMergeDateTimeStampInfo = GetAllLastMergeDateTimeStamps(loaderInstance, targetProvider);

                    var targetColumns = entity.Attributes
                        .Where(a => a.IsMappedAttribute())
                        .Select(a => a.Name)
                        .Concat(entity.RelatedParents.Select(p => p + "Id"))
                        .Concat(loaderInstance.LoadStrategy!.Columns.Select(c => c))
                        .ToArray();

                    sourceProvider.ApplyMergeInfo(loaderSource, lastMergeDateTimeStampInfo, loaderInstance.LoadStrategy!.Columns, targetColumns);

                    await MergeNewData(source, destinationDb, destinationTable, targetColumns, loader, entity, lastMergeDateTimeStampInfo);

                    break;

                default:

                    _logger.LogError("{message}",$"Unsupported load strategy '{loaderInstance.LoadStrategy!.Type}' in loader '{loaderInstance.Name}'.");

                    break;

            };

        }
    }

    private async Task DropAndLoadData(
        IDataFlowExecutableSource<ExpandoObject> source,
        IConnectionManager destinationDb,
        string destinationTable)
    {
        var destination = new DbDestination()
        {
            ConnectionManager = destinationDb,
            TableName = destinationTable,
        };

        source.LinkTo(destination);

        SqlTask.ExecuteNonQuery(destinationDb, $"DELETE FROM {destinationTable};");

        await Network.ExecuteAsync((DataFlowExecutableSource<ExpandoObject>)source);

        int rowCount = RowCountTask.Count(destinationDb, destinationTable);

        _logger.LogInformation("...copied {rowCount} records", rowCount);
    }

    private async Task MergeNewData(
        IDataFlowExecutableSource<ExpandoObject> source,
        IConnectionManager destinationDb,
        string destinationTable,
        string[] targetColumns,
        ILoader loader,
        IEntity entity,
        LoaderMergeStates lastMergeDateTimeStampInfo
        )
    {

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
            .Take(targetColumns.Length - 1 - loader.LoadStrategy!.Columns.Length)
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
                
                foreach (var (dateColumn, mergeState) in lastMergeDateTimeStampInfo)
                {

                    if (d[dateColumn] != null)
                    {
                        var fieldValue = (DateTime)d[dateColumn]!;
                        if (fieldValue > lastMergeDateTimeStampInfo[dateColumn].LastDateLoadedUtc)
                        {
                            var changeEntry = lastMergeDateTimeStampInfo[dateColumn];
                            changeEntry.LastDateLoadedUtc = fieldValue;
                            changeEntry.Updated = true;
                            lastMergeDateTimeStampInfo[dateColumn] = changeEntry;
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
                    _logger.LogInformation($"Publishing bus message: {Name}", toSend.GetType().Name);
                    _messenger?.SendMessage(loader, toSend);
                }
                
                foreach (var (dateColumn, mergeState) in lastMergeDateTimeStampInfo)
                {

                    if (d[dateColumn] != null)
                    {
                        var fieldValue = (DateTime)d[dateColumn]!;
                        if (fieldValue > lastMergeDateTimeStampInfo[dateColumn].LastDateLoadedUtc)
                        {
                            var changeEntry = lastMergeDateTimeStampInfo[dateColumn];
                            changeEntry.LastDateLoadedUtc = fieldValue;
                            changeEntry.Updated = true;
                            lastMergeDateTimeStampInfo[dateColumn] = changeEntry;
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
            await Network.ExecuteAsync((DataFlowExecutableSource<ExpandoObject>)source);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Failed to run Merge for Entity {entity}", entity.Name);
            _logger.LogError("{message}", ex.Message);
            throw;
        }

        if (inserts == 0 && updates == 0)
        {
            if (nochanges > 0)
            {
                _logger.LogInformation(
                    "{nochanges} records found but no change found to merge, last merge at: {lastMergeDateTimeStamp}", nochanges);
            }
            else
            {
                _logger.LogInformation("...no changes found to merge");
            }

            return;
        }

        var lastMergeDateTimeStamp = lastMergeDateTimeStampInfo.Values
            .Where(v => v.Updated)
            .Select(v => v.LastDateLoadedUtc)
            .Max();

        _logger.LogInformation("{inserts} records inserted, last merge at {lastMergeDateTimeStamp}", inserts, lastMergeDateTimeStamp);

        _logger.LogInformation("{updates} records updated, last merge at {lastMergeDateTimeStamp}", updates, lastMergeDateTimeStamp);

    }

    private static LoaderMergeStates GetAllLastMergeDateTimeStamps(Loader loader, IDataProvider dataProvider)
    {
        var lastMergeDateTimeStampInfo = new LoaderMergeStates();

        foreach (var dateColumn in loader.LoadStrategy!.Columns)
        {
            var lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(loader.Name, dateColumn, dataProvider);

            lastMergeDateTimeStampInfo[dateColumn] = new MergeState()
            {
                Loader = loader.Name,
                Property = dateColumn,
                LastDateLoadedUtc = lastMergeDateTimeStamp,
            };
        }

        return lastMergeDateTimeStampInfo;
    }

    private void SetAllLastMergeDateTimeStamps(Loader loader, IDataProvider dataProvider, LoaderMergeStates lastMergeDateTimeStampInfo)
    {
        foreach (var (dateColumn, mergeState) in lastMergeDateTimeStampInfo)
        {
            if (mergeState.Updated)
            {
                SetLastMergeDateTimeStamp(loader.Name, dateColumn, mergeState.LastDateLoadedUtc, dataProvider);
            }
        }
    }


    private static DateTime GetLastMergeDateTimeStamp(string loaderName, string dateColumn, 
        IDataProvider destinationDbProvider)
    {
        var lastMergeDateTime = DateTime.MinValue;

        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);

        var findQuery = new SqlKata.Query(mergeStateTableName)
                .Where("Property", dateColumn)
                .Where("Loader", loaderName)
                .Select("LastDateLoadedUtc");

        var findSql = destinationDbProvider.SqlCompiler.Compile(findQuery).ToString();

        object? resultDate = null;
        SqlTask.ExecuteReader(destinationDbProvider.ConnectionManager, findSql, r => resultDate = r);
        if (resultDate is not null)
        {
            return (DateTime)resultDate;
        }

        var insertQuery = new SqlKata.Query(mergeStateTableName).AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoadedUtc = lastMergeDateTime,
            Updated = 1
        });

        var insertSql = destinationDbProvider.SqlCompiler.Compile(insertQuery).ToString();

        SqlTask.ExecuteNonQuery(destinationDbProvider.ConnectionManager, insertSql);

        return lastMergeDateTime;

    }

    private bool SetLastMergeDateTimeStamp(
        string loaderName, string dateColumn, DateTime lastMergeDateTime,
        IDataProvider destinationDbProvider)
    {
        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);

        _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

        var updateQuery = new SqlKata.Query(mergeStateTableName)
          .Where("Property", dateColumn)
          .Where("Loader", loaderName)
          .AsUpdate(
          new
          {
              LastDateLoadedUtc = lastMergeDateTime
          });

        var updateSql = destinationDbProvider.SqlCompiler.Compile(updateQuery).ToString();

        var result = SqlTask.ExecuteNonQuery(destinationDbProvider.ConnectionManager, updateSql);

        return result == 1;
    }

}