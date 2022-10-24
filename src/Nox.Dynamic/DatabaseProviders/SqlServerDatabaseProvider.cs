using ETLBox.Connection;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Nox.Dynamic.MetaData;
using SqlKata.Compilers;
using System.Data.SqlClient;

namespace Nox.Dynamic.DatabaseProviders
{
    internal class SqlServerDatabaseProvider : IDatabaseProvider
    {

        private string _connectionString;
        
        private IConnectionManager _connectionManager;

        private readonly Compiler _sqlCompiler;

        public string ConnectionString
        {
            get { return _connectionString; }
         
            set { SetConnectionString(value); }
        }
        
        public IConnectionManager ConnectionManager => _connectionManager;

        public Compiler SqlCompiler => _sqlCompiler;

        public SqlServerDatabaseProvider(IServiceDatabase serviceDb, string applicationName)
        {


            if (serviceDb != null)
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

                _connectionManager = new SqlConnectionManager(_connectionString);
            }
            else
            {
                _connectionString = "";

                _connectionManager = new SqlConnectionManager();
            }

            _sqlCompiler = new SqlServerCompiler();
        }

        private void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;

            _connectionManager = new SqlConnectionManager(connectionString);
        }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public string ToTableNameForSql(Entity entity)
        {
            return $"[{entity.Schema}].[{entity.Table}]";
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

        public IGlobalConfiguration ConfigureHangfire(IGlobalConfiguration configuration)
        {
            configuration.UseSqlServerStorage(_connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                SchemaName = "cron",
                PrepareSchemaIfNecessary = true,
            });
            return configuration;
        }
    }
}
