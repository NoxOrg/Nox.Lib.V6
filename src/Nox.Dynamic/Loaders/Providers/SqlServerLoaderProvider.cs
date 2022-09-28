using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Dto;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace Nox.Dynamic.Loaders.Providers
{
    internal class SqlServerLoaderProvider
    {

        private readonly IConfiguration _configuration;

        private readonly ILogger _logger;

        public SqlServerLoaderProvider(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;

            _logger = logger;
        }

        public async Task<bool> ExecuteLoadersAsync(Service service)
        {
            var dbDefinition = service.Database;

            var loaders = service.Loaders;

            var entities = service.Entities;

            var connectionString = dbDefinition.ConnectionString!;

            using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            foreach (var (_,loader) in loaders)
            {
                var entity = entities[loader.Target.Entity];
                
                await LoadDataFromSource(connection, loader, entity);
            }

            await UpdateMetaDataTables(connection, service);

            return true;
        }

        private async Task UpdateMetaDataTables(SqlConnection connectionTarget, Service service)
        {
            var type = service.GetType();
            var table = $"[meta].[{type.Name}]";
            var props = type.GetProperties();

            var columns = string.Join(',', props.Select(p => $"[{p.Name}]").ToArray());
            var valuePrarams = string.Join(',', props.Select(p => $"@{p.Name}").ToArray());

            var sql = @$"INSERT INTO {table} ({columns}) VALUES ({valuePrarams});";

            using var metaCommand = new SqlCommand(sql, connectionTarget);

            foreach (var p in props)
            {
                metaCommand.Parameters.AddWithValue($"@{p.Name}", p.GetValue(service)?.ToString());
            }

            await metaCommand.ExecuteNonQueryAsync();

        }

        private async Task LoadDataFromSource(SqlConnection connectionTarget, Loader loader, Entity entity)
        {
            foreach (var source in loader.Sources)
            {
                var sourceConnectionString = _configuration[source.ConnectionVariable];

                if (sourceConnectionString is not null)
                {
                    using var connectionSource = new SqlConnection(sourceConnectionString);
                    
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

        private static async Task TruncateTargetTable(SqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new SqlCommand($"TRUNCATE TABLE [{entity.Schema}].[{entity.Table}];", connectionTarget);

            await targetCommand.ExecuteNonQueryAsync();
        }

        private static async Task<int> CountRows(SqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new SqlCommand($"SELECT COUNT(*) FROM [{entity.Schema}].[{entity.Table}];", connectionTarget);

            return Convert.ToInt32(await targetCommand.ExecuteScalarAsync());
        }

        private async Task<bool> CopyDataFromSourceToTarget(
            SqlConnection connectionSource,
            SqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity)
        {

            using var sourceCommand = new SqlCommand(loaderSource.Query, connectionSource);

            var reader = await sourceCommand.ExecuteReaderAsync();

            using var transaction = await connectionTarget.BeginTransactionAsync() as SqlTransaction;

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

            var targetColumns = entity.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

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
            catch (SqlException)
            {
                await transaction.RollbackAsync();
            }

            return true;
        }

        private async Task<bool> MergeDataFromSourceToTarget(
            SqlConnection connectionSource,
            SqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity,
            Loader loader)
        {

            var newLastMergeDateTimeStamp = new Dictionary<string, (DateTime LastMergeDateTimeStamp, bool Updated)>();

            var containsWhere = Regex.IsMatch(loaderSource.Query, @"\s+WHERE\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var query = containsWhere ? $"SELECT * FROM ({loaderSource.Query}) AS [tmp] WHERE 1=0" : $"{loaderSource.Query} WHERE 1=0"; 

            var sb = new StringBuilder(query);

            foreach (var dateColumn in loader.LoadStrategy.Columns)
            {
                var lastMergeDateTimeStamp = await GetLastMergeDateTimeStamp(connectionTarget, entity, loader.Name, dateColumn);

                sb.Append($" OR ([{dateColumn}] IS NOT NULL AND [{dateColumn}] > '{lastMergeDateTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")}')");

                newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp=lastMergeDateTimeStamp, Updated = false };
            }

            var finalQuery = sb.ToString();

            using var sourceCommand = new SqlCommand(finalQuery, connectionSource);

            var reader = await sourceCommand.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                _logger.LogInformation("...no changes found to merge");
                return false;
            }

            var primaryKeyProp = entity.Properties.Where(p => p.IsPrimaryKey).First();

            var targetColumns = entity.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

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

            using var transaction = await connectionTarget.BeginTransactionAsync() as SqlTransaction;

            if (transaction is null)
            {
                return false;
            }

            try
            {
                using var cmdUpsert = new SqlCommand(upsertSql, connectionTarget, transaction);
    
                var recordsUpserted = 0;
    
                while (await reader.ReadAsync())
                {
                    foreach(var (dateColumn, (lastMergeDateTimeStamp,updated)) in newLastMergeDateTimeStamp)
                    {
                        var dateValue = reader[dateColumn];
    
                        if (dateValue == DBNull.Value) continue;
    
                        var date = (DateTime)dateValue;
    
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
            catch (SqlException)
            {
                await transaction.RollbackAsync();
            }

            foreach (var (dateColumn, (lastMergeDateTimeStamp, updated)) in newLastMergeDateTimeStamp)
            {
                if (updated)
                {
                    await SetLastMergeDateTimeStamp(connectionTarget, entity, loader.Name, dateColumn, lastMergeDateTimeStamp);
                }
            }

            return true;

        }

        private async Task<DateTime> GetLastMergeDateTimeStamp(SqlConnection connectionTarget, Entity entity, string loaderName, string dateColumn)
        {
            DateTime lastMergeDateTime = new DateTime(1900,1,1);

            using var findCommand = new SqlCommand(
                @$"SELECT [LastDateLoaded] FROM [meta].[{Constants.Database.MergeStateTable}] 
                   WHERE [Loader]=@loaderName AND [Property]=@dateColumn;"
                , connectionTarget);

            findCommand.Parameters.AddWithValue("@loaderName", loaderName);
            findCommand.Parameters.AddWithValue("@dateColumn", dateColumn);

            var result = await findCommand.ExecuteScalarAsync();

            if (result is null)
            {
                using var insertCommand = new SqlCommand(
                    @$"INSERT INTO [meta].[{Constants.Database.MergeStateTable}] (Loader,Property,LastDateLoaded) 
                       VALUES (@loaderName,@dateColumn,@lastMergeDateTime);"
                    , connectionTarget);

                insertCommand.Parameters.AddWithValue("@loaderName", loaderName);
                insertCommand.Parameters.AddWithValue("@dateColumn", dateColumn);
                insertCommand.Parameters.AddWithValue("@lastMergeDateTime", lastMergeDateTime);

                await insertCommand.ExecuteNonQueryAsync();

                return lastMergeDateTime;
            }

            return (DateTime)result;
        }

        private async Task<bool> SetLastMergeDateTimeStamp(SqlConnection connectionTarget, Entity entity, string loaderName, string dateColumn, DateTime lastMergeDateTime)
        {

            _logger.LogInformation("...setting last merge date for {loaderName}.{dateColumn} to {lastMergeDateTime}", loaderName, dateColumn, lastMergeDateTime);

            using var targetCommand = new SqlCommand(
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
