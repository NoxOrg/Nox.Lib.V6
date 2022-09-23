using Microsoft.Extensions.Configuration;
using Nox.Dynamic.Dto;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void TruncateTargetTable(SqlConnection connectionTarget, Entity entity)
        {
            using var targetCommand = new SqlCommand($"TRUNCATE TABLE [{entity.Schema}].[{entity.Table}];", connectionTarget);

            targetCommand.ExecuteNonQuery();
        }

        private int CountRows(SqlConnection connectionTarget, Entity entity)
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

            var newLastMergeDateTimeStamp = new Dictionary<string, DateTime>();

            var sb = new StringBuilder($"{loaderSource.Query} WHERE 1=0");

            foreach (var dateColumn in loader.LoadStrategy.Columns)
            {
                var lastMergeDateTimeStamp = GetLastMergeDateTimeStamp(connectionTarget, entity, loader.Name, dateColumn);

                sb.Append($" OR ([{dateColumn}] IS NOT NULL AND [{dateColumn}] > '{lastMergeDateTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")}')");

                newLastMergeDateTimeStamp[dateColumn] = lastMergeDateTimeStamp;
            }

            using var sourceCommand = new SqlCommand(sb.ToString(), connectionSource);

            var reader = sourceCommand.ExecuteReader();

            var targetColumns = entity.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var sourceColumns = reader.GetColumnSchema().Select(c => c.ColumnName).Where(c => targetColumns.Contains(c));

            while (reader.Read())
            {
                foreach(var (dateColumn, lastMergeDateTimeStamp) in newLastMergeDateTimeStamp)
                {
                    var dateValue = reader[dateColumn];

                    if (dateValue == DBNull.Value) continue;

                    var date = (DateTime)dateValue;

                    if (date > lastMergeDateTimeStamp)
                    { 
                        newLastMergeDateTimeStamp[dateColumn] = lastMergeDateTimeStamp;
                    }
                }
            }

        }

        private DateTime GetLastMergeDateTimeStamp(SqlConnection connectionTarget, Entity entity, string loaderName, string dateColumn)
        {
            DateTime lastMergeDateTime = new DateTime(1900,1,1);

            using var targetCommand = new SqlCommand($"SELECT [LastDateLoaded] FROM [meta].[LastDataMergedState] WHERE [Loader]='{loaderName}' AND [Property]='{dateColumn}';", connectionTarget);

            var result = targetCommand.ExecuteScalar();

            if (result is null)
            {
                using var insertCommand = new SqlCommand($"INSERT INTO [meta].[LastDataMergedState] (Loader,Property,LastDateLoaded) VALUES (@loader,@property,@lastdateloaded);", connectionTarget);

                insertCommand.Parameters.Add("@loader", System.Data.SqlDbType.NVarChar).Value = loaderName;
                insertCommand.Parameters.Add("@property", System.Data.SqlDbType.NVarChar).Value = dateColumn;
                insertCommand.Parameters.Add("@lastdateloaded", System.Data.SqlDbType.DateTime2).Value = lastMergeDateTime;

                insertCommand.ExecuteNonQuery();
            }


            return lastMergeDateTime;
        }
    }
}
