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
using System.Text.Json;
using Nox.Dynamic.Models;
using Nox.Dynamic.ExtendedAttributes;
using System.ComponentModel.DataAnnotations.Schema;
using Nox.Dynamic.DatabaseProviders;

namespace Nox.Dynamic.OData.Models
{
    public class DynamicModel
    {
        private readonly IConfiguration _config;

        private readonly ILogger<DynamicModel> _logger;

        private readonly IEdmModel _edmModel;

        private readonly Dictionary<string, DynamicDbEntity> _dynamicDbEntities = new();

        private readonly DynamicService _dynamicService;

        private readonly IDatabaseProvider _databaseProvider;

        public DynamicModel(IConfiguration config, ILogger<DynamicModel> logger)
        {
            _config = config;

            _logger = logger;

            _dynamicService = new DynamicService.Builder()
                .WithLogger(_logger)
                .WithConfiguration(_config)
                .FromRootFolder(_config["Nox:DefinitionRootPath"])
                .Build();

            _databaseProvider = _dynamicService.ServiceDatabase.DatabaseProvider!;

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

            // Check database

            var dbContext = new DynamicDbContext(this);

            var model = dbContext.Model;

            dbContext.Database.EnsureCreated();

            _dynamicService.ExecuteDataLoadersAsync().GetAwaiter().GetResult();

        }

        public IDatabaseProvider GetDatabaseProvider() => _databaseProvider;

        public ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder)
        {
            foreach (var (key,entity) in _dynamicDbEntities)
            {
                modelBuilder.Entity(entity.Type, b => {

                    b.ToTable(entity.Entity.Table, entity.Entity.Schema);

                    foreach(var attr in entity.Entity.Attributes)
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
                            b.HasKey( new string[] { attr.Name });

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

            AddMetadataFromNamespace(modelBuilder, typeof(Service), "meta");

            AddMetadataFromNamespace(modelBuilder, typeof(XtendedAttributeValue), "dbo");

            return modelBuilder;
        }

        private void AddMetadataFromNamespace(ModelBuilder modelBuilder, Type typeInNamespace, string schema)
        {

            var nsTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == typeInNamespace.Namespace)
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
                            b.Property(prop.Name).HasPrecision(9,6);
                        }
                        else if (typeString == "object")
                        {
                            b.Property(prop.Name)
                            .HasColumnType(_databaseProvider
                            .ToDatabaseColumnType( new EntityAttribute() {Type = "object"} ));
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

        public object PostDynamicObject(DbContext context, string dbSetName, JsonElement obj)
        {
            var parameters = new object[] { obj };

            var ret = _dynamicDbEntities[dbSetName].DbContextPostMethod.Invoke(context, parameters);

            return ret!;
        }

        public DynamicService Configuration => _dynamicService;

        private Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)> GetTablesAndTypeBuilders()
        {
            var entities = _dynamicService.Entities;

            var aName = new AssemblyName("DynamicPoco");

            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            var dynamicTypes = new Dictionary<string, (Entity Entity, TypeBuilder TypeBuilder)>();

            foreach (var (_,entity) in entities)
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
