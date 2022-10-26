using ETLBox.Connection;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Nox.Dynamic.MetaData;
using Npgsql;
using SqlKata.Compilers;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class PostgresDatabaseProvider : IDatabaseProvider
    {
        private string _connectionString;

        private IConnectionManager _connectionManager;

        private readonly Compiler _sqlCompiler;

        public IConnectionManager ConnectionManager => _connectionManager;
        public Compiler SqlCompiler => _sqlCompiler;
        public string ConnectionString
        {
            get { return _connectionString; }

            set { SetConnectionString(value); }
        }



        public PostgresDatabaseProvider(IServiceDatabase serviceDb, string applicationName)
        {
            if (serviceDb != null)
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

                _connectionManager = new PostgresConnectionManager(_connectionString);

            }
            else
            {
                _connectionString = "";

                _connectionManager = new PostgresConnectionManager();
            }

            _sqlCompiler = new PostgresCompiler();
        }

        private void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;

            _connectionManager = new PostgresConnectionManager(connectionString);
        }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

        public string ToTableNameForSql(Entity entity)
        {
            return $"\"{entity.Schema}\".\"{entity.Table}\"";
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

        public IGlobalConfiguration ConfigureHangfire(IGlobalConfiguration configuration)
        {
            configuration.UsePostgreSqlStorage(_connectionString, new PostgreSqlStorageOptions
            {
                SchemaName = "jobs",
                PrepareSchemaIfNecessary = true,
            });
            return configuration;
        }
    }
}
