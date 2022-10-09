using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Loaders.Providers;
using Nox.Dynamic.MetaData;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class PostgresDatabaseProvider : IDatabaseProvider
    {
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        public PostgresDatabaseProvider(IServiceDatabase serviceDb, string applicationName)
        {
            NpgsqlConnectionStringBuilder csb;

            if (string.IsNullOrEmpty(serviceDb.ConnectionString))
            {
                csb = new NpgsqlConnectionStringBuilder(serviceDb.Options)
                {
                    Host = serviceDb.Server,
                    Port = serviceDb.Port,
                    Username = serviceDb.User,
                    Password = serviceDb.Password,
                    Database = serviceDb.Name,
                };
            }
            else
            {
                csb = new NpgsqlConnectionStringBuilder(serviceDb.ConnectionString);
            }

            csb.ApplicationName = applicationName;

            _connectionString = serviceDb.ConnectionString = csb.ToString();

        }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

        public string ToDatabaseColumnType(EntityAttribute entityAttribute)
        {
            var propType = entityAttribute.Type?.ToLower() ?? "string";
            var propWidth = entityAttribute.MaxWidth < 1 ? "max" : entityAttribute.MaxWidth.ToString();
            var propPrecision = entityAttribute.Precision.ToString();
            var isFixedWidth = entityAttribute.MaxWidth == entityAttribute.MinWidth;

            //     "real" => typeof(Single),
            //     "float" => typeof(Single),
            //     "bigreal" => typeof(Double),
            //     "bigfloat" => typeof(Double),

            return propType switch
            {
                "string" => isFixedWidth ? $"char({propWidth})" : $"varchar({propWidth})",
                "varchar" => $"varchar({propWidth})",
                "nvarchar" => $"varchar({propWidth})",
                "url" => "varchar(2048)",
                "email" => "varchar(320)",
                "char" => $"char({propWidth})",
                "guid" => "uuid",
                "date" => "date",
                "datetime" => "timestamp with time zone",
                "time" => "timestamp without time zone",
                "timespan" => "timestamp without time zone",
                "bool" => "boolean",
                "boolean" => "boolean",
                "object" => "jsonb",
                "int" => "integer",
                "uint" => "integer",
                "bigint" => "bigint",
                "smallint" => "smallint",
                "decimal" => $"decimal({propWidth},{propPrecision})",
                "money" => $"decimal({propWidth},{propPrecision})",
                "smallmoney" => $"decimal({propWidth},{propPrecision})",
                _ => "varchar"
            };
        }

        public async Task<bool> LoadData(Service service, ILogger logger)
        {
            var loaderProvider = new PostgresLoaderProvider(logger);

            return await loaderProvider.ExecuteLoadersAsync(service);
        }

    }
}
