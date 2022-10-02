using Humanizer;
using Humanizer.Inflections;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Nox.Dynamic.Dto;
using Nox.Dynamic.Services;
using Nox.Dynamic.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Nox.Dynamic.OData.Models
{
    public class DynamicModel
    {
        private readonly IConfiguration _config;

        private readonly ILogger<DynamicModel> _logger;

        private readonly IEdmModel _edmModel;

        private readonly Dictionary<string, DynamicDbEntity> _dynamicDbEntities = new();

        private readonly DynamicService _dynamicService;

        public DynamicModel(IConfiguration config, ILogger<DynamicModel> logger)
        {
            _config = config;

            _logger = logger;

            _dynamicService = GetDynamicService();

            var builder = new ODataConventionModelBuilder();

            var methods = typeof(DynamicDbContext).GetMethods();

            var dbContextGetCollectionMethod = 
                methods.First(m => m.Name == nameof(DynamicDbContext.GetDynamicTypedCollection));

            var dbContextGetSingleResultMethod = 
                methods.First(m => m.Name == nameof(DynamicDbContext.GetDynamicTypedSingleResult));

            var dbContextGetObjectPropertyMethod =
                methods.First(m => m.Name == nameof(DynamicDbContext.GetDynamicTypedObjectProperty));

            var dbContextGetNavigationMethod =
                methods.First(m => m.Name == nameof(DynamicDbContext.GetDynamicTypedNavigation));

            var dbContextPostMethod =
                methods.First(m => m.Name == nameof(DynamicDbContext.PostDynamicTypedObject));

            foreach (var (entityName, (entity, typeBuilder)) in GetTablesAndTypeBuilders())
            {
                var t = typeBuilder.CreateType();

                var entityType = builder.AddEntityType(t);

                var pluralName = entityName.Pluralize();

                builder.AddEntitySet(pluralName, entityType);

                _dynamicDbEntities[pluralName] = new DynamicDbEntity()
                {
                    Name = entityName,
                    PluralName = pluralName,
                    TypeBuilder = typeBuilder,
                    Type = t!,
                    Entity = entity,
                    DbContextGetCollectionMethod = dbContextGetCollectionMethod.MakeGenericMethod(t!),
                    DbContextGetSingleResultMethod = dbContextGetSingleResultMethod.MakeGenericMethod(t!),
                    DbContextGetObjectPropertyMethod = dbContextGetObjectPropertyMethod.MakeGenericMethod(t!),
                    DbContextGetNavigationMethod = dbContextGetNavigationMethod.MakeGenericMethod(t!),
                    DbContextPostMethod = dbContextPostMethod.MakeGenericMethod(t!)
                };
            }

            _edmModel = builder.GetEdmModel();

        }

        public string GetDatabaseConnectionString() => _dynamicService.DatabaseConnectionString() ?? "";

        public ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder)
        {
            foreach (var (key,value) in _dynamicDbEntities)
            {
                modelBuilder.Entity(value.Type, b => {

                    b.ToTable(value.Entity.Name);

                    // Modelvalue.Value field is an 'object' - needs new design for eg. Postgress

                    value.Entity.Properties.Where(c => c.Type.ToLower().Equals("object")).ToList().ForEach(
                        c => b.Property(c.Name).HasColumnType("sql_variant")
                    );

                });
            }

            return modelBuilder;
        }

        public IEdmModel GetEdmModel()
        {
            return _edmModel;
        }

        public IQueryable GetDynamicCollection(DbContext context, string dbSetName)
        {
            var q = _dynamicDbEntities[dbSetName].DbContextGetCollectionMethod.Invoke(context, null) as IQueryable;

            return q!;
        }

        public object GetDynamicSingleResult(DbContext context, string dbSetName, object id)
        {
            var parameters = new object[] { id };

            var ret = _dynamicDbEntities[dbSetName].DbContextGetSingleResultMethod.Invoke(context, parameters);

            return ret!;
        }
        public object GetDynamicObjectProperty(DbContext context, string dbSetName, object id, string propName)
        {
            var parameters = new object[] { id, propName };

            var ret = _dynamicDbEntities[dbSetName].DbContextGetObjectPropertyMethod.Invoke(context, parameters);

            return ret!;
        }

        public object GetDynamicNavigation(DbContext context, string dbSetName, object id, string navName)
        {
            var parameters = new object[] { id, navName };

            var ret = _dynamicDbEntities[dbSetName].DbContextGetNavigationMethod.Invoke(context, parameters);

            return ret!;
        }

        public object PostDynamicObject(DbContext context, string dbSetName, JsonElement obj)
        {
            var parameters = new object[] { obj };

            var ret = _dynamicDbEntities[dbSetName].DbContextPostMethod.Invoke(context, parameters);

            return ret!;
        }

        public DynamicService Configuration => _dynamicService;

        private DynamicService GetDynamicService()
        {
            var dynamicService = new DynamicService.Builder()
                .WithLogger(_logger)
                .WithConfiguration(_config)
                .FromRootFolder(_config["Nox:DefinitionRootPath"])
                .Build();

            dynamicService.ValidateDatabaseSchemaAsync().GetAwaiter().GetResult();

            dynamicService.ExecuteDataLoadersAsync().GetAwaiter().GetResult();
            
            return dynamicService;
        }

        private Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)> GetTablesAndTypeBuilders()
        {
            var entities = _dynamicService.Entities;

            var aName = new AssemblyName("DynamicPoco");

            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            var dynamicTypes = new Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)>();

            foreach (var (_,entity) in entities)
            {
                var entityName = entity.Name.TrimStart('_');

                TypeBuilder tb = mb.DefineType(entityName, TypeAttributes.Public, null);

                tb.AddInterfaceImplementation(typeof(IDynamicEntity));

                foreach (var col in entity.Properties)
                {
                    tb.AddPublicGetSetProperty(col.Name, col.NetDataType());
                }

                dynamicTypes.Add(entityName, (entity, tb));

            }

            foreach (var (key, entity) in entities)
            {
                var entityName = entity.Name.TrimStart('_');

                var tb = dynamicTypes[entityName].TypeBuilder;

                foreach (var col in entity.Properties)
                {
                    if (col.Name.Equals("Id")) continue;

                    if (!col.Name.EndsWith("Id")) continue;

                    var relatedEntityName = col.Name[..^2];

                    if (relatedEntityName.Equals("Entity")) continue; // Compound PK (ModelEntityId+EntityId). Skip for now..

                    var relatedTb = dynamicTypes[relatedEntityName].TypeBuilder;

                    tb.AddPublicGetSetProperty(relatedEntityName, relatedTb);

                    relatedTb.AddPublicGetSetPropertyAsList(entity.PluralName ?? entityName.Pluralize(), tb);
                }
            }

            return dynamicTypes;

        }
    }

}
