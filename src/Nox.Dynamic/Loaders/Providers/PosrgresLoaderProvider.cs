using Microsoft.Extensions.Logging;
using Nox.Dynamic.MetaData;
using Npgsql;
using System.Text;
using System.Text.RegularExpressions;

namespace Nox.Dynamic.Loaders.Providers
{
    internal class PostgresLoaderProvider
    {

        private readonly ILogger _logger;

        public PostgresLoaderProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> ExecuteLoadersAsync(Service service)
        {
            var dbDefinition = service.Database;

            var loaders = service.Loaders;

            var entities = service.Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            var connectionString = dbDefinition.ConnectionString!;

            using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync();

            foreach (var loader in loaders)
            {
                var entity = entities[loader.Target.Entity];
                
                await LoadDataFromSource(connection, loader, entity);
            }

            return true;
        }
        private async Task LoadDataFromSource(NpgsqlConnection connectionTarget, Loader loader, Entity entity)
        {
            foreach (var source in loader.Sources)
            {
                if (source.ConnectionString is not null)
                {
                    using var connectionSource = new NpgsqlConnection(source.ConnectionString);
                    
                    await connectionSource.OpenAsync();

                    var loadStrategy = loader.LoadStrategy.Type.Trim().ToLower();

                    switch (loadStrategy)
                    {
                        case "dropandload":
                            _logger.LogInformation("Dropping and loading data for entity {entity}...", entity.Name);
                            await TruncateTargetTable(connectionTarget, entity);
                            await CopyDataFromSourceToTarget(connectionSource, connectionTarget, source, entity);
                            break;

                        case "mergenew":
                            _logger.LogInformation("Merging new data for entity {entity}...", entity.Name);
                            await MergeDataFromSourceToTarget(connectionSource, connectionTarget, source, entity, loader);
                            break;

                        default:
                            break;

                    };
                }
            }
        }

        private static async Task TruncateTargetTable(NpgsqlConnection connectionTarget, Entity entity)
        {
            var sql = $"DELETE FROM [{entity.Schema}].[{entity.Table}];";

            using var targetCommand = new NpgsqlCommand(sql, connectionTarget);

            await targetCommand.ExecuteNonQueryAsync();
        }

        private static async Task<int> CountRows(NpgsqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new NpgsqlCommand($"SELECT COUNT(*) FROM [{entity.Schema}].[{entity.Table}];", connectionTarget);

            return Convert.ToInt32(await targetCommand.ExecuteScalarAsync());
        }

        private async Task<bool> CopyDataFromSourceToTarget(
            NpgsqlConnection connectionSource,
            NpgsqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity)
        {

            using var sourceCommand = new NpgsqlCommand(loaderSource.Query, connectionSource);

            var reader = await sourceCommand.ExecuteReaderAsync();

            using var transaction = await connectionTarget.BeginTransactionAsync() as NpgsqlTransaction;

            if (transaction is null)
            {
                return false;
            }

            var bulkCopy = new SqlBulkCopy(connectionTarget, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = $"[{entity.Schema}].[{entity.Table}]",
                BatchSize = 10000,
                NotifyAfter = 10000,
                BulkCopyTimeout = 0
            };

            bulkCopy.SqlRowsCopied += (sender,e) => _logger.LogInformation("...bulk copying {count} records", e.RowsCopied);

            var targetColumns = entity.Attributes.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach(var col in (await reader.GetColumnSchemaAsync()).Select(c => c.ColumnName))
            {
                if (targetColumns.Contains(col))
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(col, col));
                }
            }
            try
            {
                await bulkCopy.WriteToServerAsync(reader);

                await transaction.CommitAsync();

                _logger.LogInformation("...copied {count} records", await CountRows(connectionTarget, entity));
            }
            catch (NpgsqlException)
            {
                await transaction.RollbackAsync();
            }

            return true;
        }

        private async Task<bool> MergeDataFromSourceToTarget(
            NpgsqlConnection connectionSource,
            NpgsqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity,
            Loader loader)
        {

            var newLastMergeDateTimeStamp = new Dictionary<string, (DateTimeOffset LastMergeDateTimeStamp, bool Updated)>();

            var containsWhere = Regex.IsMatch(loaderSource.Query, @"\s+WHERE\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var query = containsWhere ? $"SELECT * FROM ({loaderSource.Query}) AS [tmp] WHERE 1=0" : $"{loaderSource.Query} WHERE 1=0"; 

            var sb = new StringBuilder(query);

            foreach (var dateColumn in loader.LoadStrategy.Columns)
            {
                var lastMergeDateTimeStamp = await GetLastMergeDateTimeStamp(connectionTarget, loader.Name, dateColumn);

                sb.Append($" OR ([{dateColumn}] IS NOT NULL AND [{dateColumn}] > '{lastMergeDateTimeStamp:yyyy-MM-dd HH:mm:ss.fff}')");

                newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp=lastMergeDateTimeStamp, Updated = false };
            }

            var finalQuery = sb.ToString();

            using var sourceCommand = new NpgsqlCommand(finalQuery, connectionSource);

            var reader = await sourceCommand.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                _logger.LogInformation("...no changes found to merge");
                return false;
            }

            var primaryKeyProp = entity.Attributes.Where(p => p.IsPrimaryKey).First();

            var targetColumns = entity.Attributes.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var matchingSourceColumns = (await reader.GetColumnSchemaAsync()).Select(c => c.ColumnName).Where(c => targetColumns.Contains(c)).ToArray();

            var matchingSourceColumsSql = string.Join(',', matchingSourceColumns);

            var sourceInsertParameters = String.Join(',', matchingSourceColumns.Select(c => $"@{c}").ToArray());

            var sourceUpdateParameters = String.Join(',', matchingSourceColumns.Where(c => !c.Equals(primaryKeyProp.Name)).Select(c => $"[{c}]=@{c}").ToArray());

            var matchedCount = matchingSourceColumns.Length;

            var upsertSql = $@"
                UPDATE [{entity.Schema}].[{entity.Table}] WITH (UPDLOCK, SERIALIZABLE) 
                    SET {sourceUpdateParameters} 
                    WHERE [{primaryKeyProp.Name}]=@{primaryKeyProp.Name};
                 
                IF @@ROWCOUNT = 0
                BEGIN
                  INSERT INTO [{entity.Schema}].[{entity.Table}] ({matchingSourceColumsSql}) VALUES ({sourceInsertParameters})
                END
            ";

            using var transaction = await connectionTarget.BeginTransactionAsync() as NpgsqlTransaction;

            if (transaction is null)
            {
                return false;
            }

            try
            {
                using var cmdUpsert = new NpgsqlCommand(upsertSql, connectionTarget, transaction);
    
                var recordsUpserted = 0;
    
                while (await reader.ReadAsync())
                {
                    foreach(var (dateColumn, (lastMergeDateTimeStamp,updated)) in newLastMergeDateTimeStamp)
                    {
                        var dateValue = reader[dateColumn];
    
                        if (dateValue == DBNull.Value) continue;

                        // TODO: Check if it is DateTime or DateTimeOffset and cast appropriately

                        var date = new DateTimeOffset((DateTime)dateValue,new TimeSpan());
    
                        if (date > lastMergeDateTimeStamp)
                        {
                            newLastMergeDateTimeStamp[dateColumn] = new() {LastMergeDateTimeStamp = date, Updated = true};
                        }
                    }
    
                    cmdUpsert.Parameters.Clear();
    
                    foreach (var columnName in matchingSourceColumns)
                    {
                        cmdUpsert.Parameters.AddWithValue($"@{columnName}", reader[columnName]);
                    }
    
                    recordsUpserted += await cmdUpsert.ExecuteNonQueryAsync();
    
                }
    
                await transaction.CommitAsync();
    
                _logger.LogInformation("...updated {count} records", recordsUpserted);
            }
            catch (NpgsqlException)
            {
                await transaction.RollbackAsync();
                throw;
            }

            foreach (var (dateColumn, (lastMergeDateTimeStamp, updated)) in newLastMergeDateTimeStamp)
            {
                if (updated)
                {
                    await SetLastMergeDateTimeStamp(connectionTarget, loader.Name, dateColumn, lastMergeDateTimeStamp);
                }
            }

            return true;

        }

        private static async Task<DateTimeOffset> GetLastMergeDateTimeStamp(NpgsqlConnection connectionTarget, string loaderName, string dateColumn)
        {
            var lastMergeDateTime = new DateTimeOffset(1900,1,1,0,0,0,new TimeSpan());

            using var findCommand = new NpgsqlCommand(
                @$"SELECT [LastDateLoaded] FROM [meta].[{Constants.Database.MergeStateTable}] 
                   WHERE [Loader]=@loaderName AND [Property]=@dateColumn;"
                , connectionTarget);

            findCommand.Parameters.AddWithValue("@loaderName", loaderName);
            findCommand.Parameters.AddWithValue("@dateColumn", dateColumn);

            var result = await findCommand.ExecuteScalarAsync();

            if (result is null)
            {
                using var insertCommand = new NpgsqlCommand(
                    @$"INSERT INTO [meta].[{Constants.Database.MergeStateTable}] (Loader,Property,LastDateLoaded) 
                       VALUES (@loaderName,@dateColumn,@lastMergeDateTime);"
                    , connectionTarget);

                insertCommand.Parameters.AddWithValue("@loaderName", loaderName);
                insertCommand.Parameters.AddWithValue("@dateColumn", dateColumn);
                insertCommand.Parameters.AddWithValue("@lastMergeDateTime", lastMergeDateTime);

                await insertCommand.ExecuteNonQueryAsync();

                return lastMergeDateTime;
            }

            return (DateTimeOffset)result;
        }

        private async Task<bool> SetLastMergeDateTimeStamp(NpgsqlConnection connectionTarget, string loaderName, 
            string dateColumn, DateTimeOffset lastMergeDateTime)
        {

            _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

            using var targetCommand = new NpgsqlCommand(
                @$"UPDATE [meta].[{Constants.Database.MergeStateTable}] SET [LastDateLoaded]=@lastMergeDateTime
                   WHERE [Loader]=@loaderName AND [Property]=@dateColumn;"
                , connectionTarget);


            targetCommand.Parameters.AddWithValue("@loaderName", loaderName);
            targetCommand.Parameters.AddWithValue("@dateColumn", dateColumn);
            targetCommand.Parameters.AddWithValue("@lastMergeDateTime", lastMergeDateTime);

            var result = await targetCommand.ExecuteNonQueryAsync();

            return result == 1;
        }

    }
}
