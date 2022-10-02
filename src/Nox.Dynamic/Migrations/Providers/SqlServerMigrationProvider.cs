using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Dto;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;

namespace Nox.Dynamic.Migrations.Providers
{
    internal class SqlServerMigrationProvider
    {

        private readonly ILogger _logger;

        public SqlServerMigrationProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> ValidateDatabaseSchemaAsync(Service service)
        {
            var masterConnectionString = ConnectionStringToMaster(service.Database);
            
            using (var connection = new SqlConnection(masterConnectionString))
            {
                await connection.OpenAsync();
                await EnsureDatabaseExists(connection, service.Database.Name);
            }

            var connectionString = service.Database.ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await ValidateTables(connection, service.Entities);
            }

            return true;
        }

        private static string ConnectionStringToMaster(ServiceDatabase dbDefinition)
        {
            var csb = new SqlConnectionStringBuilder(dbDefinition.ConnectionString)
            {
                InitialCatalog = "master"
            };

            return csb.ConnectionString;
        }

        private async Task EnsureDatabaseExists(SqlConnection connection, string databaseName)
        {
            if (!await CheckDatabaseExists(connection, databaseName))
            {
                await CreateDatabase(connection, databaseName);
                await UseDatabase(connection, databaseName);
                await CreateMetaSchema(connection);
            }
        }

        private async Task<bool> CheckDatabaseExists(SqlConnection connection, string databaseName)
        {
            _logger.LogInformation("Checking if database {databaseName} exists", databaseName);

            var qry = $"SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = @database";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
        }

        private async Task CreateDatabase(SqlConnection connection, string databaseName)
        {
            _logger.LogInformation("Creating database {databaseName}", databaseName);

            var qry = $"CREATE DATABASE [{databaseName}];";

            using var cmd = new SqlCommand(qry, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task UseDatabase(SqlConnection connection, string databaseName)
        {
            _logger.LogInformation("Setting current database to {databaseName}", databaseName);

            var qry = $"USE [{databaseName}];";

            using var cmd = new SqlCommand(qry, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task CreateMetaSchema(SqlConnection connection)
        {
            _logger.LogInformation("Creating schema [meta]");

            var qry = $"CREATE SCHEMA [meta];";

            using var cmd = new SqlCommand(qry, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ValidateTables(SqlConnection connection, Dictionary<string, Entity> entities)
        {
            AddRelationshipProperties(entities);

            foreach (var (_,entity) in entities)
            {
                await EnsureTableExists(connection, entity);
            }

            await EnsureMetadataTablesExist(connection);

        }

        private void AddRelationshipProperties(Dictionary<string, Entity> entities)
        {
            _logger.LogInformation("Resolving entity relationships");

            foreach (var (name,entity) in entities)
            {
                entity.Table ??= name;
        
                foreach (var parent in entity.RelatedParents)
                {
                    var parentEntity = entities[parent];
                    
                    var parentPK = parentEntity.Properties.Where(p => p.IsPrimaryKey).First();
                    
                    var fkName = parentPK.Name.StartsWith(parentEntity.Name, StringComparison.OrdinalIgnoreCase) 
                        ? parentPK.Name
                        : $"{parentEntity.Name}{parentPK.Name}" ;

                    _logger.LogInformation("...resolving relationship {child}({childFk}) -> {parent}({parentPk})", 
                        entity.Name, fkName, parentEntity.Name, parentPK.Name );

                    entity.Properties.Add(new Property()
                    {
                        Name = fkName,
                        Type = parentPK.Type,
                        Description = parentPK.Description,
                        IsPrimaryKey = false,
                        IsForeignKey = true,
                        IsRequired = true,
                        IsUnicode = parentPK.IsUnicode,
                        CanFilter = true,
                        CanSort = true,
                        MinWidth = parentPK.MinWidth,
                        MaxWidth = parentPK.MaxWidth,
                        MinValue = parentPK.MinValue,
                        MaxValue = parentPK.MaxValue,
                        Default = parentPK.Default,
                    });
                }
            }
        }

        private async Task EnsureMetadataTablesExist(SqlConnection connection)
        {
            var metaClasses = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == "Nox.Dynamic.Dto"
                    select t;

            foreach (var metaClass in metaClasses)
            {
                var metadataTable = new Entity()
                {
                    Name = metaClass.Name,
                    Description = "Metadata table",
                    Schema = "meta",
                    Table = metaClass.Name,
                    Properties = new()
                    {
                        new() { Name = "Id", Type = "int", IsPrimaryKey = true, IsAutoNumber = true },
                    }
                };

                _logger.LogInformation("Creating storage for metadata {meta}", metaClass.Name);

                AddClassProperties(metadataTable.Properties, metaClass);

                await EnsureTableExists(connection, metadataTable);

            }




        }

        private static void AddClassProperties(List<Property> properties, Type metaClass)
        {
            var classProperties = metaClass.GetProperties();
            foreach (var prop in classProperties)
            {
                /*
                if (prop.PropertyType.Name.StartsWith("List`"))
                {
                    continue;
                }
                */

                var p = new Property() { 
                    Name = prop.Name,
                    Type = prop.PropertyType.Name,
                    IsRequired = true,
                };

                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    p.IsRequired = false;
                }

                properties.Add(p);
            }

        }

        private async Task EnsureTableExists(SqlConnection connection, Entity entity)
        {
            if (!await CheckTableExists(connection, entity))
            {
                await CreateTable(connection, entity);
            }
        }

        private async Task<bool> CheckTableExists(SqlConnection connection, Entity entity)
        {
            _logger.LogInformation("Checking table {schemaAndTableName} exists", $"[{entity.Schema}].[{entity.Table}]");

            var qry = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schemaname AND TABLE_NAME = @tablename";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@schemaname", System.Data.SqlDbType.NVarChar).Value = entity.Schema;

            cmd.Parameters.Add("@tablename", System.Data.SqlDbType.NVarChar).Value = entity.Table;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
        }

        private async Task CreateTable(SqlConnection connection, Entity entity)
        {
            _logger.LogInformation("...creating table {schemaAndTableName}", $"[{entity.Schema}].[{entity.Table}]");

            var sqlCode = new StringBuilder();

            sqlCode.Append( $"CREATE TABLE [{entity.Schema}].[{entity.Table}] (");

            var fieldPK = string.Empty;

            foreach (var prop in entity.Properties)
            {
                var fieldName = prop.Name;
                var fieldSqlType = MapToSqlType(prop);
                var nullValue = prop.IsPrimaryKey || prop.IsRequired ? " NOT NULL" : " NULL";
                var identity = prop.IsAutoNumber && prop.IsPrimaryKey && fieldSqlType == "int" ? " IDENTITY" : "";
                
                sqlCode.Append($"[{fieldName}] {fieldSqlType}{identity}{nullValue},");

                if (prop.IsPrimaryKey)
                {
                    fieldPK = prop.Name;
                }
            }

            sqlCode.AppendLine($" CONSTRAINT [PK_{entity.Table}] PRIMARY KEY ([{fieldPK}]) );");

            var qryCreate = sqlCode.ToString();

            using var cmdCreate = new SqlCommand(qryCreate, connection);

            await cmdCreate.ExecuteNonQueryAsync();
        }

        private static string MapToSqlType(Property prop)
        {
            var propType = prop.Type?.ToLower() ?? "string";
            var propWidth = prop.MaxWidth < 1 ? "max" : prop.MaxWidth.ToString();
            var propPrecision = prop.Precision.ToString();

           //     "real" => typeof(Single),
           //     "float" => typeof(Single),
           //     "bigreal" => typeof(Double),
           //     "bigfloat" => typeof(Double),

            return propType switch
            {
                "string"    => prop.IsUnicode ? $"nvarchar({propWidth})" : $"varchar({propWidth})",
                "varchar"   => $"varchar({propWidth})",
                "nvarchar"  => $"nvarchar({propWidth})",
                "url"       => "varchar(2048)",
                "email"     => "varchar(320)",
                "char"      => prop.IsUnicode ? $"nchar({propWidth})" : $"char({propWidth})",
                "guid"      => "guid",
                "date"      => "date",
                "datetime"  => "datetimeoffset",
                "time"      => "time",
                "timespan"  => "timespan",
                "bool"      => "bit",
                "boolean"   => "bit",
                "object"    => "sql_variant",
                "int"       => "int",
                "uint"      => "uint",
                "bigint"    => "bigint",
                "smallint"  => "smallint",
                "decimal"   => $"decimal({propWidth},{propPrecision})",
                "money"     => $"decimal({propWidth},{propPrecision})",
                "smallmoney"=> $"decimal({propWidth},{propPrecision})",
                _ => "nvarchar(max)"
            };
        }

    }
}
