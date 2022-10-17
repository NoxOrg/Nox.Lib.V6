using ETLBox.Connection;
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

internal class LoaderExecuter
{

    private readonly ILogger _logger;

    public LoaderExecuter(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Service service)
    {
        var destinationDbProvider = service.Database.DatabaseProvider!;

        var compiler = destinationDbProvider.SqlCompiler;

        var loaders = service.Loaders;

        var entities = service.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        var sortedEntities = EntitiesSortedByDependancy(entities);

        //foreach (var loader in LoadersSortedByDependancy(loaders, entities))
        //{
        //    var entity = entities[loader.Target.Entity];

        //    await LoadDataFromSource(destinationDbProvider, loader, entity);
        //}

        foreach (var entity in sortedEntities)
        {
            var loader = loaders.First(l => l.Target.Entity == entity.Name);
            await LoadDataFromSource(destinationDbProvider, loader, entity, compiler);
        }

        return true;
    }


    private ICollection<Loader> LoadersSortedByDependancy(
        ICollection<Loader> loaders,
        IDictionary<string, Entity> entitiesDictionary)
    {
        return loaders.OrderBy(l => entitiesDictionary[l.Target.Entity].SortOrder).ToList();
    }


    // TODO: Move to Service.cs
    // Note to Andre: Parked since we still need a better way to manage the sort at Drop and Load. >>>>> backlog  :(
    private ICollection<Entity> EntitiesSortedByDependancy(
        IDictionary<string, Entity> entitiesDictionary)
    {
        var entities = entitiesDictionary.Values.ToList();

        foreach (var entity in entities)
        {
            foreach (var parent in entity.RelatedParents)
            {
                entities
                    .First(x => x.Name.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .RelatedChildren
                    .Add(entity.Name);
            }
        }

        // rough sort
        entities.Sort((entity1, entity2) =>
            entity1.RelatedParents.Count.CompareTo(entity2.RelatedParents.Count));

        // heirachy sort to place entities in dependency order
        var i = 0;
        var sortedEntities = new List<Entity>();
        while (entities.Count > 0)
        {
            var count = CountParentsInSortedEntities(entities, sortedEntities, i);

            if (count == entities[i].RelatedParents.Count)
            {
                sortedEntities.Add(entities[i]);
                entities.RemoveAt(i);
                i = 0;
            }
            else
            {
                if (++i >= entities.Count)
                {
                    i = 0;
                }
            }
        }

        i = 1;
        foreach (var e in sortedEntities)
        {
            e.SortOrder = i++;
        }

        return sortedEntities;

    }

    private static int CountParentsInSortedEntities(
            List<Entity> unsortedEntities,
            List<Entity> sortedEntities,
            int iteration)
    {
        var result = 0;

        foreach (string p in unsortedEntities[iteration].RelatedParents)
        {
            result += sortedEntities.Count(x => x.Name.Equals(p));
        }

        return result;
    }


    private async Task LoadDataFromSource(IDatabaseProvider destinationDbProvider,
        Loader loader, Entity entity, Compiler compiler)
    {
        var destinationDb = destinationDbProvider.ConnectionManager;
        var destinationTable = destinationDbProvider.ToTableNameForSql(entity);

        foreach (var loaderSource in loader.Sources)
        {
            if (loaderSource.DatabaseProvider is not null)
            {
                var sourceDb = loaderSource.DatabaseProvider.ConnectionManager;

                var loadStrategy = loader.LoadStrategy.Type.Trim().ToLower();

                switch (loadStrategy)
                {
                    case "dropandload":
                        _logger.LogInformation("Dropping and loading data for entity {entity}...", entity.Name);

                        await DropAndLoadData(sourceDb, destinationDb, loaderSource, destinationTable);

                        break;

                    case "mergenew":
                        _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);

                        await MergeNewData(sourceDb, destinationDb, loaderSource, loader, destinationTable, entity, compiler);

                        break;

                    default:

                        await Task.Delay(1);

                        break;

                };
            }
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
        Compiler compiler)
    {
        var newLastMergeDateTimeStamp = new Dictionary<string, (DateTimeOffset LastMergeDateTimeStamp, bool Updated)>();

        var query = $"SELECT {string.Join(',', entity.Attributes.Where(a => a.IsMappedAttribute).Select(a => a.Name))}  FROM ({loaderSource.Query}) AS [tmp] WHERE 1=0";

        var sb = new StringBuilder(query);
        var lastMergeDateTimeStamp = DateTimeOffset.MinValue;

        foreach (var dateColumn in loader.LoadStrategy.Columns)
        {
            lastMergeDateTimeStamp = GetLastMergeDateTimeStampAsync(destinationDb, loader.Name, dateColumn, compiler);

            sb.Append($" OR ([{dateColumn}] IS NOT NULL AND [{dateColumn}] > '{lastMergeDateTimeStamp:yyyy-MM-dd HH:mm:ss.fff}')");

            newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp = lastMergeDateTimeStamp, Updated = false };
        }

        var finalQuery = sb.ToString();

        var source = new DbSource()
        {
            ConnectionManager = sourceDb,
            Sql = finalQuery,
        };

        var destination = new DbMerge(destinationDb, destinationTable);

        destination.MergeProperties.IdColumns =
            entity.Attributes.Where(a => a.IsPrimaryKey).Select(o => new IdColumn() { IdPropertyName = o.Name }).ToArray();
        destination.MergeProperties.CompareColumns =
            entity.Attributes.Where(a => !a.IsPrimaryKey && a.IsMappedAttribute).Select(o => new CompareColumn() { ComparePropertyName = o.Name }).ToArray();

        destination.CacheMode = ETLBox.DataFlow.Transformations.CacheMode.Partial;
        destination.MergeMode = MergeMode.InsertsAndUpdates;

        source.LinkTo(destination);

        var analatics = new CustomDestination();

        int inserts = 0;
        int updates = 0;
        int nochanges = 0;

        analatics.WriteAction = (row, _) =>
        {
            dynamic r = row as ExpandoObject;
            if (r.ChangeAction == ChangeAction.Insert)
            {
                inserts++;
            }
            else if (r.ChangeAction == ChangeAction.Update)
            {
                updates++;
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
            SetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, timeStamp, compiler);
        }
        return true;

    }


    private DateTimeOffset GetLastMergeDateTimeStampAsync(IConnectionManager destinationDb, string loaderName, string dateColumn, Compiler compiler)
    {
        var lastMergeDateTime = new DateTimeOffset(1900, 1, 1, 0, 0, 0, new TimeSpan());

        var destination = new DbDestination()
        {
            ConnectionManager = destinationDb,
            TableName = Constants.Database.MergeStateTable,
        };

        var findQuery = new Query(
                $"meta.{Constants.Database.MergeStateTable}")
                .Where("Property", dateColumn)
                .Where("Loader", loaderName)
                .Select("LastDateLoaded");

        var findSql = compiler.Compile(findQuery).ToString();

        object? resultDate = null;
        SqlTask.ExecuteReader(destinationDb, findSql, r => resultDate = r);
        if (resultDate is not null)
        {
            return new DateTimeOffset((DateTime)resultDate);
        }

        var insertQuery = new Query($"meta.{Constants.Database.MergeStateTable}").AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoaded = lastMergeDateTime
        });

        var insertSql = compiler.Compile(insertQuery).ToString();

        SqlTask.ExecuteNonQuery(destinationDb, insertSql);

        return lastMergeDateTime;

    }

    private bool SetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName,
        string dateColumn, DateTimeOffset lastMergeDateTime, Compiler compiler)
    {

        _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

        var updateQuery = new Query($"meta.{Constants.Database.MergeStateTable}")
          .Where("Property", dateColumn)
          .Where("Loader", loaderName)
          .AsUpdate(
          new
          {
              LastDateLoaded = lastMergeDateTime
          });

        var updateSql = compiler.Compile(updateQuery).ToString();

        var result = SqlTask.ExecuteNonQuery(destinationDb, updateSql);

        return result == 1;
    }

}
