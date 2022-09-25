using Microsoft.Extensions.Configuration;
using Nox.Dynamic.Dto;
using System.Data.SqlClient;
using System.Text;

namespace Nox.Dynamic.Loaders.Providers
{
    internal class SqlServerLoaderProvider
    {

        private readonly IConfiguration _configuration;

        public SqlServerLoaderProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ExecuteLoaders(ServiceDatabase dbDefinition, Dictionary<string,Loader> loaders, Dictionary<string,Entity> entities)
        {
            var connectionString = dbDefinition.ConnectionString!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            foreach (var (_,loader) in loaders)
            {
                var entity = entities[loader.Target.Entity];
                
                LoadDataFromSource(connection, loader, entity);
            }
        }

        private void LoadDataFromSource(SqlConnection connectionTarget, Loader loader, Entity entity)
        {
            foreach (var source in loader.Sources)
            {
                var sourceConnectionString = _configuration[source.ConnectionVariable];

                if (sourceConnectionString is not null)
                {
                    using var connectionSource = new SqlConnection(sourceConnectionString);
                    
                    connectionSource.Open();

                    var loadStrategy = loader.LoadStrategy.Type.Trim().ToLower();

                    switch (loadStrategy)
                    {
                        case "dropandload":
                            TruncateTargetTable(connectionTarget, entity);
                            CopyDataFromSourceToTarget(connectionSource, connectionTarget, source, entity);
                            break;

                        case "mergenew":
                            MergeDataFromSourceToTarget(connectionSource, connectionTarget, source, entity, loader);
                            break;

                        default:
                            break;

                    };
                }
            }
        }

        private static void TruncateTargetTable(SqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new SqlCommand($"TRUNCATE TABLE [{entity.Schema}].[{entity.Table}];", connectionTarget);

            targetCommand.ExecuteNonQuery();
        }

        private static int CountRows(SqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new SqlCommand($"SELECT COUNT(*) FROM [{entity.Schema}].[{entity.Table}];", connectionTarget);

            return Convert.ToInt32(targetCommand.ExecuteScalar());
        }

        private void CopyDataFromSourceToTarget(
            SqlConnection connectionSource,
            SqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity)
        {

            using var sourceCommand = new SqlCommand(loaderSource.Query, connectionSource);

            var reader = sourceCommand.ExecuteReader();

            using SqlTransaction transaction = connectionTarget.BeginTransaction();

            var bulkCopy = new SqlBulkCopy(connectionTarget, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = $"[{entity.Schema}].[{entity.Table}]",
                BulkCopyTimeout = 0
            };

            var targetColumns = entity.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach(var col in reader.GetColumnSchema().Select(c => c.ColumnName))
            {
                if (targetColumns.Contains(col))
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(col, col));
                }
            }
            try
            {
                bulkCopy.WriteToServer(reader);

                transaction.Commit();
            }
            catch (SqlException)
            {
                transaction.Rollback();
            }
        }

        private void MergeDataFromSourceToTarget(
            SqlConnection connectionSource,
            SqlConnection connectionTarget,
            LoaderSource loaderSource,
            Entity entity,
            Loader loader)
        {

            var newLastMergeDateTimeStamp = new Dictionary<string, (DateTime LastMergeDateTimeStamp, bool Updated)>();

            var sb = new StringBuilder($"{loaderSource.Query} WHERE 1=0");

            foreach (var dateColumn in loader.LoadStrategy.Columns)
            {
                var lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(connectionTarget, entity, loader.Name, dateColumn);

                sb.Append($" OR ([{dateColumn}] IS NOT NULL AND [{dateColumn}] > '{lastMergeDateTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")}')");

                newLastMergeDateTimeStamp[dateColumn] = new() { LastMergeDateTimeStamp=lastMergeDateTimeStamp, Updated = false };
            }

            using var sourceCommand = new SqlCommand(sb.ToString(), connectionSource);

            var reader = sourceCommand.ExecuteReader();

            if (!reader.HasRows)
            {
                return;
            }

            var primaryKeyProp = entity.Properties.Where(p => p.IsPrimaryKey).First();

            var targetColumns = entity.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var matchingSourceColumns = reader.GetColumnSchema().Select(c => c.ColumnName).Where(c => targetColumns.Contains(c)).ToArray();

            var matchingSourceColumsSql = string.Join(',', matchingSourceColumns);

            var sourceInsertParameters = String.Join(',', matchingSourceColumns.Select(c => $"@{c}").ToArray());

            var sourceUpdateParameters = String.Join(',', matchingSourceColumns.Where(c => !c.Equals(primaryKeyProp.Name)).Select(c => $"[{c}]=@{c}").ToArray());

            var matchedCount = matchingSourceColumns.Length;

            var upsertSql = $@"
                BEGIN TRANSACTION;
                 
                UPDATE [{entity.Schema}].[{entity.Table}] WITH (UPDLOCK, SERIALIZABLE) 
                    SET {sourceUpdateParameters} 
                    WHERE [{primaryKeyProp.Name}]=@{primaryKeyProp.Name};
                 
                IF @@ROWCOUNT = 0
                BEGIN
                  INSERT INTO [{entity.Schema}].[{entity.Table}] ({matchingSourceColumsSql}) VALUES ({sourceInsertParameters})
                END
                 
                COMMIT TRANSACTION;
            ";


            using var cmdUpsert = new SqlCommand(upsertSql, connectionTarget);

            while (reader.Read())
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

                var upserted = cmdUpsert.ExecuteNonQuery();

            }

            foreach (var (dateColumn, (lastMergeDateTimeStamp, updated)) in newLastMergeDateTimeStamp)
            {
                if (updated)
                {
                    SetLastMergeDateTimeStamp(connectionTarget, entity, loader.Name, dateColumn, lastMergeDateTimeStamp);
                }
            }

        }

        private DateTime GetLastMergeDateTimeStamp(SqlConnection connectionTarget, Entity entity, string loaderName, string dateColumn)
        {
            DateTime lastMergeDateTime = new DateTime(1900,1,1);

            using var findCommand = new SqlCommand(
                @$"SELECT [LastDateLoaded] FROM [meta].[LastDataMergedState] 
                   WHERE [Loader]=@loader AND [Property]=@property;"
                , connectionTarget);

            findCommand.Parameters.AddWithValue("@loader", loaderName);
            findCommand.Parameters.AddWithValue("@property", dateColumn);

            var result = findCommand.ExecuteScalar();

            if (result is null)
            {
                using var insertCommand = new SqlCommand(
                    @$"INSERT INTO [meta].[LastDataMergedState] (Loader,Property,LastDateLoaded) 
                       VALUES (@loader,@property,@lastMergeDateTime);"
                    , connectionTarget);

                insertCommand.Parameters.AddWithValue("@loader", loaderName);
                insertCommand.Parameters.AddWithValue("@property", dateColumn);
                insertCommand.Parameters.AddWithValue("@lastMergeDateTime", lastMergeDateTime);

                insertCommand.ExecuteNonQuery();

                return lastMergeDateTime;
            }

            return (DateTime)result;
        }

        private bool SetLastMergeDateTimeStamp(SqlConnection connectionTarget, Entity entity, string loaderName, string dateColumn, DateTime lastMergeDateTime)
        {

            using var targetCommand = new SqlCommand(
                @$"UPDATE [meta].[LastDataMergedState] SET [LastDateLoaded]=@lastMergeDateTime
                   WHERE [Loader]=@loader AND [Property]=@property;"
                , connectionTarget);


            targetCommand.Parameters.AddWithValue("@loader", loaderName);
            targetCommand.Parameters.AddWithValue("@property", dateColumn);
            targetCommand.Parameters.AddWithValue("@lastMergeDateTime", lastMergeDateTime);

            var result = targetCommand.ExecuteNonQuery();

            return result == 1;
        }

    }
}
