using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Loaders.Providers;
using Nox.Dynamic.MetaData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class SqlServerDatabaseProvider : IDatabaseProvider
    {

        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        public SqlServerDatabaseProvider(IServiceDatabase serviceDb, string applicationName)
        {
            SqlConnectionStringBuilder csb;

            if (string.IsNullOrEmpty(serviceDb.ConnectionString))
            {
                csb = new SqlConnectionStringBuilder(serviceDb.Options)
                {
                    DataSource = $"{serviceDb.Server},{serviceDb.Port}",
                    UserID = serviceDb.User,
                    Password = serviceDb.Password,
                    InitialCatalog = serviceDb.Name,
                };
             }
            else
            {
                csb = new SqlConnectionStringBuilder(serviceDb.ConnectionString);
            }

            csb.ApplicationName = applicationName;

            _connectionString = serviceDb.ConnectionString = csb.ToString();

        }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public string ToDatabaseColumnType(EntityAttribute entityAttribute)
        {
            var propType = entityAttribute.Type?.ToLower() ?? "string";
            var propWidth = entityAttribute.MaxWidth < 1 ? "max" : entityAttribute.MaxWidth.ToString();
            var propPrecision = entityAttribute.Precision.ToString();

            //     "real" => typeof(Single),
            //     "float" => typeof(Single),
            //     "bigreal" => typeof(Double),
            //     "bigfloat" => typeof(Double),

            return propType switch
            {
                "string" => entityAttribute.IsUnicode ? $"nvarchar({propWidth})" : $"varchar({propWidth})",
                "varchar" => $"varchar({propWidth})",
                "nvarchar" => $"nvarchar({propWidth})",
                "url" => "varchar(2048)",
                "email" => "varchar(320)",
                "char" => entityAttribute.IsUnicode ? $"nchar({propWidth})" : $"char({propWidth})",
                "guid" => "uniqueidentifier",
                "date" => "date",
                "datetime" => "datetimeoffset",
                "time" => "datetime",
                "timespan" => "timespan",
                "bool" => "bit",
                "boolean" => "bit",
                "object" => "sql_variant",
                "int" => "int",
                "uint" => "uint",
                "bigint" => "bigint",
                "smallint" => "smallint",
                "decimal" => $"decimal({propWidth},{propPrecision})",
                "money" => $"decimal({propWidth},{propPrecision})",
                "smallmoney" => $"decimal({propWidth},{propPrecision})",
                _ => "nvarchar(max)"
            };
        }

        public async Task<bool> LoadData(Service service, ILogger logger)
        {
            var loaderProvider = new SqlServerLoaderProvider(logger);

            return await loaderProvider.ExecuteLoadersAsync(service);
        }
    }
}
