using Nox.Dynamic.Dto;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Nox.Dynamic.Migrations.Providers
{
    internal class SqlServerMigrationProvider
    {

        public SqlServerMigrationProvider()
        {

        }

        public static void ValidateDatabaseSchema(ServiceDatabase dbDefinition, Dictionary<string,Entity> entities)
        {
            var masterConnectionString = ConnectionString(dbDefinition, "master");
            
            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                EnsureDatabaseExists(connection, dbDefinition.Name);
            }

            var connectionString = ConnectionString(dbDefinition);
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                ValidateTables(connection, entities);
            }

            dbDefinition.ConnectionString = connectionString;
        }

        private static string ConnectionString(ServiceDatabase dbDefinition, string? db = null)
        {
            db ??= dbDefinition.Name;

            var csb = new SqlConnectionStringBuilder(dbDefinition.Options)
            {
                DataSource = dbDefinition.Server,
                UserID = dbDefinition.User,
                Password = dbDefinition.Password,
                InitialCatalog = db
            };

            return csb.ConnectionString;
        }

        private static void EnsureDatabaseExists(SqlConnection connection, string databaseName)
        {
            if (!CheckDatabaseExists(connection, databaseName))
            {
                CreateDatabase(connection, databaseName);
                UseDatabase(connection, databaseName);
                CreateMetaSchema(connection);
            }
        }

        private static bool CheckDatabaseExists(SqlConnection connection, string databaseName)
        {
            var qry = $"SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = @database";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;

            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        private static void CreateDatabase(SqlConnection connection, string databaseName)
        {
            var qry = $"CREATE DATABASE [{databaseName}];";

            using var cmd = new SqlCommand(qry, connection);

            cmd.ExecuteNonQuery();
        }
        private static void UseDatabase(SqlConnection connection, string databaseName)
        {
            var qry = $"USE [{databaseName}];";

            using var cmd = new SqlCommand(qry, connection);

            cmd.ExecuteNonQuery();
        }

        private static void CreateMetaSchema(SqlConnection connection)
        {
            var qry = $"CREATE SCHEMA [meta];";

            using var cmd = new SqlCommand(qry, connection);

            cmd.ExecuteNonQuery();
        }

        private static void ValidateTables(SqlConnection connection, Dictionary<string, Entity> entities)
        {
            AddRelationshipProperties(entities);

            foreach (var (_,entity) in entities)
            {
                EnsureTableExists(connection, entity);
            }

            var mergeState = new Entity()
            {
                Name = "LastDataMergedState",
                Description = "Tracks state between data merge oprations",
                Schema = "meta",
                Table = "LastDataMergedState",
                Properties = new()
                {
                    new() { Name = "Id", Type = "int", IsPrimaryKey = true, IsAutoNumber = true },
                    new() { Name = "Loader", Type = "string", MaxWidth = 64 },
                    new() { Name = "Property", Type = "string", MaxWidth = 64 },
                    new() { Name = "LastDateLoaded", Type = "datetime" },
                }

            };

            EnsureTableExists(connection, mergeState);

        }

        private static void AddRelationshipProperties(Dictionary<string, Entity> entities)
        {
            foreach (var (name,entity) in entities)
            {
                entity.Table ??= name;
        
                entity.Schema ??= "dbo";

                foreach (var parent in entity.RelatedParents)
                {
                    var parentEntity = entities[parent];
                    
                    var parentPK = parentEntity.Properties.Where(p => p.IsPrimaryKey).First();
                    
                    var fkName = parentPK.Name.StartsWith(parentEntity.Name, StringComparison.OrdinalIgnoreCase) 
                        ? parentPK.Name
                        : $"{parentEntity.Name}{parentPK.Name}" ;

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

        private static void EnsureTableExists(SqlConnection connection, Entity entity)
        {
            if (!CheckTableExists(connection, entity))
            {
                CreateTable(connection, entity);
            }
        }

        private static bool CheckTableExists(SqlConnection connection, Entity entity)
        {
            var qry = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schemaname AND TABLE_NAME = @tablename";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@schemaname", System.Data.SqlDbType.NVarChar).Value = entity.Schema;

            cmd.Parameters.Add("@tablename", System.Data.SqlDbType.NVarChar).Value = entity.Table;

            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        private static void CreateTable(SqlConnection connection, Entity entity)
        {
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

            cmdCreate.ExecuteNonQuery();
        }

        private static string MapToSqlType(Property prop)
        {
            var propType = prop.Type?.ToLower() ?? "string";
            var propWidth = prop.MaxWidth < 1 ? "max" : prop.MaxWidth.ToString();

            return propType switch
            {
                "string"    => prop.IsUnicode ? $"nvarchar({propWidth})" : $"varchar({propWidth})",
                "char"      => prop.IsUnicode ? $"nchar({propWidth})" : $"char({propWidth})",
                "date"      => "datetime2",
                "datetime"  => "datetime2",
                "bool"      => "bit",
                "boolean"   => "bit",
                "object"    => "sql_variant",
                "url"       => "varchar(2048)",
                "email"     => "varchar(320)",
                _ => propType
            };
        }

    }
}
