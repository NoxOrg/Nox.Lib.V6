using Nox.Dynamic.Dto;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Nox.Dynamic.Migrations.Providers
{
    internal class SqlServerMigrationProvider
    {

        public static void ValidateDatabaseSchema(ServiceDatabase dbDefinition, EntityDefinition[] entities)
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
            }
        }

        private static bool CheckDatabaseExists(SqlConnection connection, string databaseName)
        {
            var qry = $"select count(*) from master.dbo.sysdatabases where name = @database";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;

            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        private static void CreateDatabase(SqlConnection connection, string databaseName)
        {
            var qryCreate = $"CREATE DATABASE {databaseName}";

            using var cmdCreate = new SqlCommand(qryCreate, connection);

            cmdCreate.ExecuteNonQuery();
        }

        private static void ValidateTables(SqlConnection connection, EntityDefinition[] entities)
        {
            EnsureRelationshipsExist(entities);

            foreach (var entity in entities)
            {
                EnsureTableExists(connection, entity);
            }
        }

        private static void EnsureRelationshipsExist(EntityDefinition[] entities)
        {
            foreach (var entity in entities)
            {
                entity.Table ??= entity.Name;
        
                entity.Schema ??= "dbo";

                foreach (var parent in entity.RelatedParents)
                {
                    var parentEntity = entities.Where(e => e.Name.Equals(parent, StringComparison.OrdinalIgnoreCase)).First();
                    
                    var parentPK = parentEntity.Properties.Where(p => p.IsPrimaryKey).First();
                    
                    var fkName = parentPK.Name.StartsWith(parentEntity.Name, StringComparison.OrdinalIgnoreCase) 
                        ? parentPK.Name
                        : $"{parentEntity.Name}{parentPK.Name}" ;

                    entity.Properties.Add(new PropertyDefinition()
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

        private static void EnsureTableExists(SqlConnection connection, EntityDefinition entity)
        {
            if (!CheckTableExists(connection, entity))
            {
                CreateTable(connection, entity);
            }
        }

        private static bool CheckTableExists(SqlConnection connection, EntityDefinition entity)
        {
            var qry = $"select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @schemaname AND TABLE_NAME = @tablename";

            using var cmd = new SqlCommand(qry, connection);

            cmd.Parameters.Add("@schemaname", System.Data.SqlDbType.NVarChar).Value = entity.Schema;

            cmd.Parameters.Add("@tablename", System.Data.SqlDbType.NVarChar).Value = entity.Table;

            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        private static void CreateTable(SqlConnection connection, EntityDefinition entity)
        {
            var sqlCode = new StringBuilder();

            sqlCode.Append( $"CREATE TABLE [{entity.Schema}].[{entity.Table}] (");

            var fieldPK = string.Empty;
            foreach (var prop in entity.Properties)
            {
                var fieldName = prop.Name;
                var fieldSqlType = MapToSqlType(prop);
                var nullValue = prop.IsPrimaryKey || prop.IsRequired ? " NOT NULL" : " NULL";
                var identity = prop.IsPrimaryKey && fieldSqlType == "int" ? " IDENTITY" : "";
                
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

        private static string MapToSqlType(PropertyDefinition prop)
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
