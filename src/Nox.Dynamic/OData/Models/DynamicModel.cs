using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Nox.Dynamic.MetaData;
using Nox.Dynamic.Services;
using Nox.Dynamic.Extensions;
using System.Reflection;
using System.Reflection.Emit;
using Nox.Dynamic.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Hangfire;
using Nox.Dynamic.Loaders;
using Nox.Data;

namespace Nox.Dynamic.OData.Models
{
    public class DynamicModel : IDynamicModel
    {
        private readonly IConfiguration _config;

        private readonly ILogger<DynamicModel> _logger;

        private readonly IEdmModel _edmModel;

        private readonly IDynamicService _dynamicService;

        private readonly ILoaderExecutor _loaderExecutor;

        private readonly IDatabaseProvider _databaseProvider;

        private readonly Dictionary<string, DynamicDbEntity> _dynamicDbEntities = new();



        public DynamicModel(
            IConfiguration config, ILogger<DynamicModel> logger,
            IDynamicService dynamicService, ILoaderExecutor loaderExecutor)
        {
            _config = config;

            _logger = logger;

            _loaderExecutor = loaderExecutor;

            _dynamicService = dynamicService;

            _databaseProvider = _dynamicService.ServiceDatabase.DatabaseProvider!;

            var builder = new ODataConventionModelBuilder();

            var methods = typeof(IDynamicDbContext).GetMethods();

            var dbContextGetCollectionMethod =
                methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedCollection));

            var dbContextGetSingleResultMethod =
                methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedSingleResult));

            var dbContextGetObjectPropertyMethod =
                methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedObjectProperty));

            var dbContextGetNavigationMethod =
                methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedNavigation));

            var dbContextPostMethod =
                methods.First(m => m.Name == nameof(IDynamicDbContext.PostDynamicTypedObject));

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

            // Check database

            var dbContext = new DynamicDbContext(this);

            var model = dbContext.Model;

            if (dbContext.Database.EnsureCreated())
            {
                dbContext.Add(_dynamicService.Service);

                dbContext.SaveChanges();
            }

        }

        public void SetupRecurringLoaderTasks()
        {
            var executor = _loaderExecutor;

            // setup recurring jobs based on cron schedule

            foreach (var loader in _dynamicService.Loaders)
            {

                var entity = _dynamicService.Entities[loader.Target.Entity];

                if (loader.Schedule.RunOnStartup)
                {
                    executor.ExecuteLoaderAsync(loader, _databaseProvider, entity).GetAwaiter().GetResult();
                }

                RecurringJob.AddOrUpdate(
                    $"{_dynamicService.Name}.{loader.Name}",
                    () => executor.ExecuteLoaderAsync(loader, _databaseProvider, entity),
                    loader.Schedule.CronExpression
                );
            }

        }

        public IDatabaseProvider GetDatabaseProvider() => _databaseProvider;

        public IDynamicService GetDynamicService() => _dynamicService;

        public ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder)
        {
            foreach (var (key, entity) in _dynamicDbEntities)
            {
                modelBuilder.Entity(entity.Type, b =>
                {

                    b.ToTable(entity.Entity.Table, entity.Entity.Schema);

                    foreach (var attr in entity.Entity.Attributes)
                    {
                        var prop = b.Property(attr.Name);

                        var netType = attr.NetDataType();

                        prop.HasColumnType(_databaseProvider.ToDatabaseColumnType(attr));

                        if (netType.Equals(typeof(string)))
                        {
                            prop.HasMaxLength(attr.MaxWidth);
                        }

                        else if (attr.IsDateTimeType())
                        {
                            // don't set Maxwidth, throw's error on db create
                        }

                        else if (attr.MaxWidth > 0 && attr.Precision > 0)
                        {
                            prop.HasPrecision(attr.MaxWidth, attr.Precision);
                        }

                        try
                        {
                            prop.IsRequired(attr.IsRequired);
                        }
                        catch { }

                        if (attr.IsPrimaryKey)
                        {
                            b.HasKey(new string[] { attr.Name });

                            if (!attr.IsAutoNumber)
                            {
                                prop.ValueGeneratedNever();
                            }
                        }

                        if (attr.IsUnicode)
                        {
                            prop.IsUnicode();
                        }

                        if (attr.MinWidth == attr.MaxWidth)
                        {
                            prop.IsFixedLength();
                        }

                    }

                });

            }

            AddMetadataFromNamespace(modelBuilder, typeof(MetaBase), "meta");

            AddMetadataFromNamespace(modelBuilder, typeof(ModelBase), "meta");

            AddMetadataFromNamespace(modelBuilder, typeof(DatabaseBase), "meta");

            AddMetadataFromNamespace(modelBuilder, typeof(XtendedAttributeValue), "dbo");

            return modelBuilder;
        }

        private void AddMetadataFromNamespace(ModelBuilder modelBuilder, Type baseType, string schema)
        {
            var assemblyToScan = Assembly.GetAssembly(baseType);

            if (assemblyToScan == null) return;

            var nsTypes = assemblyToScan.GetTypes()
                .Where(t => t.IsSubclassOf(baseType))
                .Where(t => t.IsClass && t.IsSealed && t.IsPublic);

            foreach (var metaType in nsTypes)
            {
                modelBuilder.Entity(metaType, b =>
                {
                    b.ToTable(metaType.Name, schema);

                    var entityTypes = modelBuilder.Model.GetEntityTypes();

                    var properties = entityTypes
                        .First(e => e.ClrType.Name == metaType.Name)
                        .ClrType
                        .GetProperties();


                    foreach (var prop in properties)
                    {
                        if (prop.GetCustomAttributes(typeof(NotMappedAttribute), false).Length > 0)
                            continue;

                        var typeString = prop.PropertyType.Name.ToLower();

                        if (prop.Name == "Name" && typeString == "string")
                        {
                            b.Property(prop.Name).HasMaxLength(128);
                        }
                        else if (typeString == "decimal")
                        {
                            b.Property(prop.Name).HasPrecision(9, 6);
                        }
                        else if (typeString == "object")
                        {
                            b.Property(prop.Name)
                            .HasColumnType(_databaseProvider
                                .ToDatabaseColumnType(new EntityAttribute() { Type = "object" })
                            );
                        }
                    }
                });
            }
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

        public object PostDynamicObject(DbContext context, string dbSetName, string json)
        {
            var parameters = new object[] { json };

            var ret = _dynamicDbEntities[dbSetName].DbContextPostMethod.Invoke(context, parameters);

            return ret!;
        }

        private Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)> GetTablesAndTypeBuilders()
        {
            var entities = _dynamicService.Entities;

            var aName = new AssemblyName("DynamicPoco");

            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            var dynamicTypes = new Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)>();

            foreach (var (_, entity) in entities)
            {
                TypeBuilder tb = mb.DefineType(entity.Name, TypeAttributes.Public, null);

                tb.AddInterfaceImplementation(typeof(IDynamicEntity));

                foreach (var col in entity.Attributes)
                {
                    tb.AddPublicGetSetProperty(col.Name, col.NetDataType());
                }

                dynamicTypes.Add(entity.Name, (entity, tb));

            }

            foreach (var (key, entity) in entities)
            {
                var tb = dynamicTypes[entity.Name].TypeBuilder;

                foreach (var col in entity.Attributes)
                {
                    foreach (var relatedEntityName in entity.RelatedParents)
                    {
                        var relatedTb = dynamicTypes[relatedEntityName].TypeBuilder;

                        tb.AddPublicGetSetProperty(relatedEntityName, relatedTb);

                        relatedTb.AddPublicGetSetPropertyAsList(entity.PluralName, tb);
                    }

                }
            }

            return dynamicTypes;

        }
    }

}
