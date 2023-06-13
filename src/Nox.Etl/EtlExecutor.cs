using ETLBox.Connection;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using Microsoft.Extensions.Logging;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Models;
using Nox.Messaging;
using System.Dynamic;
using Nox.Core.Enumerations;
using Nox.Core.Exceptions;
using Nox.Core.Interfaces.Messaging.Events;
using Nox.Solution;


namespace Nox.Etl;

public class EtlExecutor : IEtlExecutor
{
    private readonly ILogger<EtlExecutor> _logger;
    private readonly IEnumerable<INoxEvent> _messages;
    private readonly INoxMessenger? _messenger;
    private readonly NoxSolution _solution;
    
    public EtlExecutor(
        ILogger<EtlExecutor> logger,
        IEnumerable<INoxEvent> messages,
        NoxSolution solution,
        INoxMessenger? messenger = null)
    {
        _logger = logger;
        _messages = messages;
        _messenger = messenger;
        _solution = solution;
    }

    public async Task<bool> ExecuteAsync(IDataProvider entityStore)
    {
        if (_solution.Application?.Integrations != null)
        {
            var integrations = _solution.Application.Integrations;

            var entities = _solution.Domain?.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
            
            foreach (var integration in integrations!)
            {
                if (integration.Target!.TargetType == EtlTargetType.Entity && entities == null)
                {
                    throw new NoxIntegrationException($"Nox domain configuration does not contain any entities. If an entity is configured as an integration target, the entity must exist in the domain configuration.");
                }

                if (!entities!.ContainsKey(integration.Target!.Name))
                {
                    throw new NoxIntegrationException($"Integration target entity does not exist. Integration {integration.Name} targets an entity {integration.Target.Name}, but it does not exist in the domain configuration.");
                }

                var entity = _solution.Domain!.Entities.First(e => e.Name.Equals(integration.Target!.Name, StringComparison.OrdinalIgnoreCase));

                await LoadDataFromSource(integration, entityStore, entity);
            }    
        }
        
        

        return true;
    }

    public async Task<bool> ExecuteEtlAsync(Integration integration, IDataProvider entityStore)
    {
        var entities = _solution.Domain?.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        if (!entities!.ContainsKey(integration.Target!.Name))
        {
            throw new NoxIntegrationException($"Integration target entity does not exist. Integration {integration.Name} targets an entity {integration.Target.Name}, but it does not exist in the domain configuration.");
        }
        var entity = _solution.Domain!.Entities.First(e => e.Name.Equals(integration.Target!.Name, StringComparison.OrdinalIgnoreCase));
        await LoadDataFromSource(integration, entityStore, entity);
        return true;
    }

    private async Task LoadDataFromSource(Integration integration, IDataProvider entityStore, Solution.Entity entity)
    {
        var targetProvider = entityStore;
        var destinationDb = targetProvider.ConnectionManager;
        var destinationTable = targetProvider.ToTableNameForSql(entity., entity.Schema);
        var destinationSqlCompiler = targetProvider.SqlCompiler;

        foreach (var loaderSource in loaderInstance.Sources!)
        {
            var sourceProvider = _solution.DataSources!.First(ds => ds.Name == loaderSource.DataSource).DataProvider!;
            var source = sourceProvider!.DataFlowSource(loaderSource);
            var loadStrategy = loaderInstance.LoadStrategy?.Type.Trim().ToLower() ?? "unknown";
            
            _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);
            await MergeNewData(source, destinationDb, destinationTable, loader, loaderSource, entity, sourceProvider, targetProvider);
        }
    }

    private async Task MergeNewData(
        IDataFlowExecutableSource<ExpandoObject> source,
        IConnectionManager destinationDb,
        string destinationTable,
        ILoader loader,
        ILoaderSource loaderSource,
        IEntity entity,
        IDataProvider sourceProvider,
        IDataProvider targetProvider
    )
    {
        var lastMergeDateTimeStampInfo = GetAllLastMergeDateTimeStamps(loader, targetProvider, entity);

        var targetColumns =
            (new string[] { entity.Key.Name })
            .Concat( entity.Attributes
                .Where(a => a.IsMappedAttribute())
                .Select(a => a.Name)
            )
            .Concat(entity.RelatedParents.Select(p => p + "Id"))
            .ToArray();

        sourceProvider.ApplyMergeInfo(loaderSource, lastMergeDateTimeStampInfo, loader.LoadStrategy!.Columns, targetColumns);
        await ExecuteMergeNewData(source, destinationDb, destinationTable, targetColumns, loader, entity, lastMergeDateTimeStampInfo);

        SetAllLastMergeDateTimeStamps(loader, targetProvider, lastMergeDateTimeStampInfo);
    }

    private async Task ExecuteMergeNewData(
        IDataFlowExecutableSource<ExpandoObject> source,
        IConnectionManager destinationDb,
        string destinationTable,
        string[] targetColumns,
        ILoader loader,
        IEntity entity,
        IntegrationMergeStates lastMergeDateTimeStampInfo
        )
    {
        var destination = new DbMerge(destinationDb, destinationTable)
        {
            CacheMode = ETLBox.DataFlow.Transformations.CacheMode.Partial,
            MergeMode = MergeMode.InsertsAndUpdates
        };

        destination.MergeProperties.IdColumns =
            targetColumns
            .Skip(0)
            .Take(1)
            .Select(colName => new IdColumn() { IdPropertyName = colName })
            .ToArray();

        foreach (var dateColumn in lastMergeDateTimeStampInfo)
        {
            if (entity.Attributes.Any(a => a.Name.Equals(dateColumn.Value.Property, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (destination.MergeProperties.CompareColumns == null) destination.MergeProperties.CompareColumns = new List<CompareColumn>();
                destination.MergeProperties.CompareColumns.Add(new CompareColumn
                {
                    ComparePropertyName = dateColumn.Value.Property
                });
            }
        }
        
        source.LinkTo(destination);

        var postProcessDestination = new CustomDestination();

        // Store analytics
        
        int inserts = 0, updates = 0, unchanged = 0;

        // Get events to fire, if any

        INoxEvent? entityCreatedMsg = null, entityUpdatedMsg = null;
        if (loader.Messaging != null && loader.Messaging.Any())
        {
            entityCreatedMsg = _messages.FindEventImplementation(entity.Name, NoxEventType.Created);
            entityUpdatedMsg = _messages.FindEventImplementation(entity.Name, NoxEventType.Updated);
        }

        postProcessDestination.WriteAction = (row, _) =>
        {
            var record = (IDictionary<string, object?>)row;

            if ((ChangeAction)record["ChangeAction"]! == ChangeAction.Insert)
            {
                inserts++;
                if(entityCreatedMsg is not null) SendChangeEvent(loader, row, entityCreatedMsg, NoxEventSource.EtlMerge);
                UpdateMergeStates(lastMergeDateTimeStampInfo, record);
            }
            else if ((ChangeAction)record["ChangeAction"]! == ChangeAction.Update)
            {
                updates++;
                if (entityUpdatedMsg is not null) SendChangeEvent(loader, row, entityUpdatedMsg, NoxEventSource.EtlMerge);
                UpdateMergeStates(lastMergeDateTimeStampInfo, record);
            }
            else if ((ChangeAction)record["ChangeAction"]! == ChangeAction.Exists)
            {
                unchanged++;
            }
        };

        destination.LinkTo(postProcessDestination);

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

        LogMergeAnalytics(inserts, updates, unchanged, lastMergeDateTimeStampInfo);
    }

    private void SendChangeEvent(ILoader loader, ExpandoObject row, INoxEvent message, NoxEventSource eventSource)
    {
        var toSend = message.MapInstance(row, eventSource);

        _logger.LogInformation("Publishing bus message: {Name}", toSend.GetType().Name);
        if (loader.Messaging != null)
        {
            _messenger?.SendMessage(loader.Messaging, toSend).Wait();    
        }
    }

    private static void UpdateMergeStates(IntegrationMergeStates lastMergeDateTimeStampInfo, IDictionary<string, object?> record)
    {
        foreach (var dateColumn in lastMergeDateTimeStampInfo.Keys)
        {
            if (record.TryGetValue(dateColumn, out var dateColumnValue))
            {
                if (dateColumnValue == null) continue;

                if (DateTime.TryParse(dateColumnValue.ToString(), out var fieldValue))
                {
                    if (fieldValue > lastMergeDateTimeStampInfo[dateColumn].LastDateLoadedUtc)
                    {
                        var changeEntry = lastMergeDateTimeStampInfo[dateColumn];
                        changeEntry.LastDateLoadedUtc = fieldValue;
                        changeEntry.Updated = true;
                        lastMergeDateTimeStampInfo[dateColumn] = changeEntry;
                    }
                }
            }
            else
            {
                if (record.TryGetValue("ChangeDate", out var changeDate))
                {
                    if (changeDate == null) continue;
                    if (DateTime.TryParse(changeDate.ToString(), out var changeDateValue))
                    {
                        var changeEntry = lastMergeDateTimeStampInfo[EtlExecutorConstants.DefaultMergeProperty];
                        changeEntry.LastDateLoadedUtc = changeDateValue.ToUniversalTime();
                        changeEntry.Updated = true;
                        lastMergeDateTimeStampInfo[EtlExecutorConstants.DefaultMergeProperty] = changeEntry;    
                    }
                }
            }
        }
    }

    private static IntegrationMergeStates GetAllLastMergeDateTimeStamps(ILoader loader, IDataProvider dataProvider, IEntity entity)
    {
        var lastMergeDateTimeStampInfo = new IntegrationMergeStates();

        var addedMergeColumn = false;
        
        foreach (var dateColumn in loader.LoadStrategy!.Columns)
        {
            if (entity.Attributes.Any(a => a.Name.Equals(dateColumn, StringComparison.InvariantCultureIgnoreCase)))
            {
                addedMergeColumn = true;
                var lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(loader.Name, dateColumn, dataProvider);

                lastMergeDateTimeStampInfo[dateColumn] = new MergeState()
                {
                    Loader = loader.Name,
                    Property = dateColumn,
                    LastDateLoadedUtc = lastMergeDateTimeStamp,
                };    
            }
            
        }

        if (!addedMergeColumn)
        {
            var lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(loader.Name, EtlExecutorConstants.DefaultMergeProperty, dataProvider);
            lastMergeDateTimeStampInfo[EtlExecutorConstants.DefaultMergeProperty] = new MergeState()
            {
                Loader = loader.Name,
                Property = EtlExecutorConstants.DefaultMergeProperty,
                LastDateLoadedUtc = lastMergeDateTimeStamp,
            };
            RemoveEntityMergeDateTimeStamps(loader.Name, dataProvider);
        }
        else
        {
            RemoveDefaultMergeDateTimeStamp(loader.Name, dataProvider);
        }

        return lastMergeDateTimeStampInfo;
    }

    private void SetAllLastMergeDateTimeStamps(ILoader loader, IDataProvider dataProvider, IntegrationMergeStates lastMergeDateTimeStampInfo)
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
        var lastMergeDateTime = NoxDateTime.MinSqlDate;

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
            if (DateTime.TryParse(resultDate!.ToString(), out var result))
            {
                return result;
            }
        }

        var insertQuery = new SqlKata.Query(mergeStateTableName).AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoadedUtc = lastMergeDateTime,
            Updated = '1'
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

    private static void RemoveDefaultMergeDateTimeStamp(string loaderName, IDataProvider destinationDbProvider)
    {
        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);
        var deleteQuery = new SqlKata.Query(mergeStateTableName)
            .Where("Loader", loaderName)
            .Where("Property", EtlExecutorConstants.DefaultMergeProperty)
            .AsDelete();
        var deleteSql = destinationDbProvider.SqlCompiler.Compile(deleteQuery).ToString();
        SqlTask.ExecuteNonQuery(destinationDbProvider.ConnectionManager, deleteSql);
    }
    
    private static void RemoveEntityMergeDateTimeStamps(string loaderName, IDataProvider destinationDbProvider)
    {
        var mergeStateTableName = destinationDbProvider.ToTableNameForSqlRaw(DatabaseObject.MergeStateTableName, DatabaseObject.MetadataSchemaName);
        var deleteQuery = new SqlKata.Query(mergeStateTableName)
            .Where("Loader", loaderName)
            .Where("Property", "!=", EtlExecutorConstants.DefaultMergeProperty)
            .AsDelete();
        var deleteSql = destinationDbProvider.SqlCompiler.Compile(deleteQuery).ToString();
        SqlTask.ExecuteNonQuery(destinationDbProvider.ConnectionManager, deleteSql);
    }

    private void LogReloadAnalytics(int inserts)
    {
        if (inserts == 0)
        {
            _logger.LogInformation("...no records found to load");
            return;
        }
        _logger.LogInformation($"{inserts} records inserted, last merge at {DateTime.Now}");
    }
    
    private void LogMergeAnalytics(int inserts, int updates, int unchanged, IntegrationMergeStates lastMergeDateTimeStampInfo)
    {
        var lastMergeDateTimeStamp = DateTime.MinValue;
        
        var info = lastMergeDateTimeStampInfo.Values.Select(v => v.LastDateLoadedUtc);

        if (info.Any())
        {
            lastMergeDateTimeStamp = info.Max();
        }

        if (inserts == 0 && updates == 0)
        {
            if (unchanged > 0)
            {
                _logger.LogInformation(
                    "{nochanges} records found but no change found to merge, last merge at: {lastMergeDateTimeStamp}", unchanged, lastMergeDateTimeStamp);
            }
            else
            {
                _logger.LogInformation("...no changes found to merge");
            }

            return;
        }

        var changes = lastMergeDateTimeStampInfo.Values
            .Where(v => v.Updated)
            .Select(v => v.LastDateLoadedUtc);

        if (changes.Any())
        {
            lastMergeDateTimeStamp = changes.Max();
        }

        _logger.LogInformation("{inserts} records inserted, last merge at {lastMergeDateTimeStamp}", inserts, lastMergeDateTimeStamp);

        _logger.LogInformation("{updates} records updated, last merge at {lastMergeDateTimeStamp}", updates, lastMergeDateTimeStamp);

        return;
    }

}