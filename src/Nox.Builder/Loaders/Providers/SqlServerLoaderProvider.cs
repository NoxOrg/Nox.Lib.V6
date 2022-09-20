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

        public void ExecuteLoaders(ServiceDatabase dbDefinition, LoaderDefinition[] loaders)
        {
            var connectionString = dbDefinition.ConnectionString!;

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            foreach (var loader in loaders)
            {
                LoadDataFromSource(connection, loader);
            }
        }

        private void LoadDataFromSource(SqlConnection connectionTarget, LoaderDefinition loader)
        {
            foreach (var source in loader.Sources)
            {
                var sourceConnectionString = _configuration[source.ConnectionVariable];

                if (sourceConnectionString is not null)
                {
                    using var connectionSource = new SqlConnection(sourceConnectionString);
                    connectionSource.Open();
                    if (loader.Target is not null)
                    {
                        loader.Target.Schema ??= "dbo";
                        TransferDataFromSourceToTarget(connectionSource, connectionTarget, source, $"[{loader.Target.Schema}].[{loader.Target.Table}]");
                    }
                }
            }
        }

        private void TransferDataFromSourceToTarget(SqlConnection connectionSource,
            SqlConnection connectionTarget,
            LoaderSource loaderSource,
            string targetTable)
        {
            using var sourceCommand = new SqlCommand(loaderSource.Query, connectionSource);

            var reader = sourceCommand.ExecuteReader();

            using SqlTransaction transaction = connectionTarget.BeginTransaction();

            var bulkCopy = new SqlBulkCopy(connectionTarget, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = targetTable,
                BulkCopyTimeout = 0
            };

            foreach(var col in reader.GetColumnSchema().Select(c => c.ColumnName))
            {
                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(col, col));
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
    }
}
