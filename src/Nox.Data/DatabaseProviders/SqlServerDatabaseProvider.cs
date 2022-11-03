using ETLBox.Connection;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SqlKata.Compilers;
using System.Data.SqlClient;

namespace Nox.Data
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {

        private string _connectionString = string.Empty;
        
        private readonly IConnectionManager _connectionManager = new SqlConnectionManager();

        private readonly Compiler _sqlCompiler = new SqlServerCompiler();

        public string ConnectionString
        {
            get { return _connectionString; }
         
            set { SetConnectionString(value); }
        }

        public string Name => "sqlserver";

        public IConnectionManager ConnectionManager => _connectionManager;

        public Compiler SqlCompiler => _sqlCompiler;

        public void ConfigureServiceDatabase(IServiceDatabase serviceDb, string applicationName)
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

            serviceDb.ConnectionString = csb.ToString();

            SetConnectionString(serviceDb.ConnectionString);
        }

        private void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;

            _connectionManager.ConnectionString = new SqlConnectionString(_connectionString);
        }

        public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.UseSqlServer(_connectionString);
        }

        public string ToDatabaseColumnType(IEntityAttribute entityAttribute)
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

        public IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration)
        {
            configuration.UseSqlServerStorage(_connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                SchemaName = "jobs",
                PrepareSchemaIfNecessary = true,
            });
            return configuration;
        }

        public string ToTableNameForSql(string table, string schema)
        {
            return $"[{schema}].[{table}]";
        }

        public string ToTableNameForSqlRaw(string table, string schema)
        {
            return $"{schema}.{table}";
        }

        public EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema)
        {
            builder.ToTable(table,schema);
            return builder;
        }
    }
}
