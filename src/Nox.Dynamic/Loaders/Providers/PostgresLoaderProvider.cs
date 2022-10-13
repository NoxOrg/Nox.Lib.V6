﻿using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using ETLBox.DataFlow;
using ETLBox.DataFlow.Connectors;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.MetaData;
using Npgsql;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nox.Dynamic.Loaders.Providers;

internal class PostgresLoaderProvider // LoaderExecuter
{

    private readonly ILogger _logger;
    private static readonly PostgresCompiler _compiler = new PostgresCompiler();

    public PostgresLoaderProvider(ILogger logger)
    {
        _logger = logger; 
    }

    public async Task<bool> ExecuteLoadersAsync(Service service) //ExecuteAsync
    {
        var destinationDbProvider = service.Database.DatabaseProvider!;

        using var metaDbConnection = new NpgsqlConnection(service.Database.DatabaseProvider!.ConnectionString);

        await metaDbConnection.OpenAsync();

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
            await LoadDataFromSource(destinationDbProvider, metaDbConnection, loader, entity);
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


    private async Task LoadDataFromSource(IDatabaseProvider destinationDbProvider, NpgsqlConnection metaDbConnection,
        Loader loader, Entity entity)
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

                        await MergeNewData(sourceDb, destinationDb, metaDbConnection, loaderSource, loader, destinationTable, entity);

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
        NpgsqlConnection metaDbConnection,

        LoaderSource loaderSource,
        Loader loader,
        string destinationTable, 
        Entity entity)
    {
        var newLastMergeDateTimeStamp = new Dictionary<string, (DateTimeOffset LastMergeDateTimeStamp, bool Updated)>();

        //var containsWhere = Regex.IsMatch(loaderSource.Query, @"\s+WHERE\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        //int pFrom = loaderSource.Query.IndexOf("SELECT", StringComparison.InvariantCultureIgnoreCase) + "SELECT".Length;
        //int pTo = loaderSource.Query.IndexOf("FROM", StringComparison.InvariantCultureIgnoreCase);


        //var loaderColumns = loaderSource.Query.Substring(pFrom, pTo - pFrom).Split(',').ToList();

        //var selectColumns = new List<string>();
        //foreach (var loaderColumn in loaderColumns)
        //{
        //    if(loaderColumn.Contains("AS", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        var index = loaderColumn.IndexOf("AS", StringComparison.InvariantCultureIgnoreCase) + "AS".Length;
        //        selectColumns.Add(loaderColumn.Substring(index));
        //        continue;
        //    }
        //    selectColumns.Add(loaderColumn);
        //}


        var query = $"SELECT {string.Join(',', entity.Attributes.Where(a => a.IsMappedAttribute).Select(a => a.Name))}  FROM ({loaderSource.Query}) AS [tmp] WHERE 1=0";

        var sb = new StringBuilder(query);
        var lastMergeDateTimeStamp = DateTimeOffset.MinValue;

        foreach (var dateColumn in loader.LoadStrategy.Columns)
        {
            lastMergeDateTimeStamp = GetLastMergeDateTimeStampAsync(destinationDb, loader.Name, dateColumn);

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

        analatics.WriteAction = (row, _) => {
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
        catch(Exception ex)
        {
            _logger.LogCritical("Failed to run Merge for Entity {entity} at {lastMergeDateTimeStamp}", entity.Name, lastMergeDateTimeStamp);
            _logger.LogError(ex.Message);
        }

        if (inserts==0 && updates == 0)
        {          
            if(nochanges>0)
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
            SetLastMergeDateTimeStamp(destinationDb, loader.Name, dateColumn, timeStamp);
        }


        //using var sourceCommand = new NpgsqlCommand(finalQuery, connectionSource);

        //var reader = await sourceCommand.ExecuteReaderAsync();

        //if (!reader.HasRows)
        //{
        //    _logger.LogInformation("...no changes found to merge");
        //    return false;
        //}

        //var primaryKeyProp = entity.Attributes.Where(p => p.IsPrimaryKey).First();

        //var targetColumns = entity.Attributes.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        //var matchingSourceColumns = (await reader.GetColumnSchemaAsync()).Select(c => c.ColumnName).Where(c => targetColumns.Contains(c)).ToArray();

        //var matchingSourceColumsSql = string.Join(',', matchingSourceColumns);

        //var sourceInsertParameters = String.Join(',', matchingSourceColumns.Select(c => $"@{c}").ToArray());

        //var sourceUpdateParameters = String.Join(',', matchingSourceColumns.Where(c => !c.Equals(primaryKeyProp.Name)).Select(c => $"[{c}]=@{c}").ToArray());

        //var matchedCount = matchingSourceColumns.Length;

        //var upsertSql = $@"
        //    UPDATE [{entity.Schema}].[{entity.Table}] WITH (UPDLOCK, SERIALIZABLE) 
        //        SET {sourceUpdateParameters} 
        //        WHERE [{primaryKeyProp.Name}]=@{primaryKeyProp.Name};

        //    IF @@ROWCOUNT = 0
        //    BEGIN
        //        INSERT INTO [{entity.Schema}].[{entity.Table}] ({matchingSourceColumsSql}) VALUES ({sourceInsertParameters})
        //    END
        //";

        //using var transaction = await connectionTarget.BeginTransactionAsync() as NpgsqlTransaction;

        //if (transaction is null)
        //{
        //    return false;
        //}

        //try
        //{
        //    using var cmdUpsert = new NpgsqlCommand(upsertSql, connectionTarget, transaction);

        //    var recordsUpserted = 0;

        //    while (await reader.ReadAsync())
        //    {
        //        foreach (var (dateColumn, (lastMergeDateTimeStamp, updated)) in newLastMergeDateTimeStamp)
        //        {
        //            var dateValue = reader[dateColumn];

        //            if (dateValue == DBNull.Value) continue;

        //            // TODO: Check if it is DateTime or DateTimeOffset and cast appropriately

        //            var date = new DateTimeOffset((DateTime)dateValue, new TimeSpan());

        //            if (date > lastMergeDateTimeStamp)
        //            {
        //                newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp = date, Updated = true };
        //            }
        //        }

        //        cmdUpsert.Parameters.Clear();

        //        foreach (var columnName in matchingSourceColumns)
        //        {
        //            cmdUpsert.Parameters.AddWithValue($"@{columnName}", reader[columnName]);
        //        }

        //        recordsUpserted += await cmdUpsert.ExecuteNonQueryAsync();

        //    }

        //    await transaction.CommitAsync();

        //    _logger.LogInformation("...updated {count} records", recordsUpserted);
        //}
        //catch (NpgsqlException)
        //{
        //    await transaction.RollbackAsync();
        //    throw;
        //}

        //foreach (var (dateColumn, (lastMergeDateTimeStamp, updated)) in newLastMergeDateTimeStamp)
        //{
        //    if (updated)
        //    {
        //        await SetLastMergeDateTimeStamp(metaDbConnection, loader.Name, dateColumn, lastMergeDateTimeStamp);
        //    }
        //}

        return true;

    }


    private DateTimeOffset GetLastMergeDateTimeStampAsync(IConnectionManager destinationDb, string loaderName, string dateColumn)
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

        var findSql =_compiler.Compile(findQuery).ToString();

        object? resultDate = null;
        SqlTask.ExecuteReader(destinationDb, findSql, r => resultDate = r);
        if (resultDate is not null)
        {
            return new  DateTimeOffset((DateTime)resultDate);
        }

        var insertQuery = new Query($"meta.{Constants.Database.MergeStateTable}").AsInsert(
        new
        {
            Loader = loaderName,
            Property = dateColumn,
            LastDateLoaded = lastMergeDateTime
        });

        var insertSql = _compiler.Compile(insertQuery).ToString();

        SqlTask.ExecuteNonQuery(destinationDb, insertSql);

        return lastMergeDateTime;

    }

    private bool SetLastMergeDateTimeStamp(IConnectionManager destinationDb, string loaderName,
        string dateColumn, DateTimeOffset lastMergeDateTime)
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

        var updateSql = _compiler.Compile(updateQuery).ToString();
    
        var result = SqlTask.ExecuteNonQuery(destinationDb, updateSql);

        return result == 1;
    }

}
