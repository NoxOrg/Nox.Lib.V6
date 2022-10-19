using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.MetaData;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Dynamic;
using System.Text;

namespace Nox.Dynamic.Loaders;

internal class LoaderExecutor
{

    private readonly ILogger _logger;

    public LoaderExecutor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Service service)
    {
        ETLBox.Logging.Logging.LogInstance = _logger;

        var destinationDbProvider = service.Database.DatabaseProvider!;

        var loaders = service.Loaders;

        var entities = service.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        var sortedLoaders = loaders.OrderBy(l => entities[l.Target.Entity].SortOrder).ToList();

        foreach (var loader in sortedLoaders)
        {
            await LoadDataFromSource(destinationDbProvider, loader, entities[loader.Target.Entity]);
        }

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

                    _logger.LogError("Unsupported load strategy '{loadStrategy}' in loader '{loaderName}'.",
                        loader.LoadStrategy.Type, loader.Name);

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

        var targetColumns = entity.Attributes.Where(a => a.IsMappedAttribute).Select(a => a.Name)
                .Concat(entity.RelatedParents.Select(p => p + "Id"))
                .Concat(loader.LoadStrategy.Columns.Select(c => c))
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
        
        var finalQueryParams = compiledQuery.NamedBindings.Select(nb => new QueryParameter(nb.Key,nb.Value));

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

        // TODO: We are not extracting and storing the MAX of the dates retrieved.

        source.LinkTo(destination);

        var analatics = new CustomDestination();

        int inserts = 0;
        int updates = 0;
        int nochanges = 0;

        analatics.WriteAction = (row, _) =>
        {
            dynamic r = row as ExpandoObject;
            IDictionary<string, object?> d = new Dictionary<string, object?>(row);

            if (r.ChangeAction == ChangeAction.Insert)
            {
                inserts++;
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
            await Network.ExecuteAsync(source);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Failed to run Merge for Entity {entity} at {lastMergeDateTimeStamp}", entity.Name, lastMergeDateTimeStamp);
            _logger.LogError(ex.Message);
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


    private DateTime GetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName, string dateColumn, Compiler compiler)
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
