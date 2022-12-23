using System.Reflection;
using System.Reflection.Emit;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Nox.Core.Extensions;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;

namespace Nox.Data;

public class DynamicModel : IDynamicModel
{
    private readonly IEdmModel _edmModel;
    private readonly IDynamicService _dynamicService;
    private readonly IDataProvider _databaseProvider;
    private readonly Dictionary<string, DynamicDbEntity> _dynamicDbEntities = new();

    public DynamicModel(ILogger<DynamicModel> logger, IDynamicService dynamicService, IEtlExecutor etlExecutor)
    {
        _dynamicService = dynamicService;

        _databaseProvider = dynamicService.MetaService.Database!.DataProvider!;

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

        _dynamicService.EnsureDatabaseCreatedIfAutoMigrationsIsSet(dbContext);
    }

    public IDataProvider GetDatabaseProvider() => _databaseProvider;

    public IDynamicService GetDynamicService() => _dynamicService;

    public ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder)
    {
        foreach (var (key, entity) in _dynamicDbEntities)
        {
            modelBuilder.Entity(entity.Type, b =>
            {
                _databaseProvider.ConfigureEntityTypeBuilder(b, entity.Entity.Table, entity.Entity.Schema);

                foreach (var attr in entity.Entity.Attributes)
                {
                    var prop = b.Property(attr.Name);

                    var netType = attr.NetDataType();

                    prop.HasColumnType(_databaseProvider.ToDatabaseColumnType(attr));

                    if (netType == typeof(string))
                    {
                        prop.HasMaxLength(attr.MaxWidth);
                    }

                    else if (attr.IsDateTimeType())
                    {
                        // don't set MaxWidth, throw's error on db create
                    }

                    else if (attr.MaxWidth > 0 && attr.Precision > 0)
                    {
                        prop.HasPrecision(attr.MaxWidth, attr.Precision);
                    }

                    try
                    {
                        prop.IsRequired(attr.IsRequired);
                    }
                    catch
                    {
                    }

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
        
        _dynamicService.AddMetadata(modelBuilder);
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

    public object PostDynamicObject(DbContext context, string dbSetName, string json)
    {
        var parameters = new object[] { json };

        var ret = _dynamicDbEntities[dbSetName].DbContextPostMethod.Invoke(context, parameters);

        return ret!;
    }

    private Dictionary<string, (IEntity Entity, TypeBuilder TypeBuilder)> GetTablesAndTypeBuilders()
    {
        var entities = _dynamicService.Entities;

        var aName = new AssemblyName("DynamicPoco");

        var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

        var mb = ab.DefineDynamicModule(aName.Name!);

        var dynamicTypes = new Dictionary<string, (IEntity Entity, TypeBuilder TypeBuilder)>();

        foreach (var (_, entity) in entities!)
        {
            var tb = mb.DefineType(entity.Name, TypeAttributes.Public, null);

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