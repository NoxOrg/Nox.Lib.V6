using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Nox.Core.Extensions;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Messaging.Events;
using Nox.Core.Models.Entity;
using System.Reflection;
using System.Reflection.Emit;

namespace Nox.Data;

public class DynamicModel : IDynamicModel
{
    private readonly IEdmModel _edmModel;
    private readonly IDynamicService _dynamicService;
    private readonly IDataProvider _databaseProvider;
    private readonly Dictionary<string, IDynamicDbEntity> _dynamicDbEntities = new();

    public DynamicModel(
        ILogger<DynamicModel> logger,
        IDynamicService dynamicService,
        IEnumerable<INoxEvent>? messages = null,
        INoxMessenger? messenger = null)
    {
        _dynamicService = dynamicService;

        _databaseProvider = dynamicService.MetaService.Database!.DataProvider!;

        var builder = new ODataConventionModelBuilder();

        var methods = typeof(IDynamicDbContext).GetMethods();

        var dbContextGetCollectionMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedCollection));

        var dbContextGetSingleResultMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedSingleResult));

        var dbContextGetObjectPropertyMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedObjectProperty));

        var dbContextGetNavigationMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.GetDynamicTypedNavigation));

        var dbContextPostMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.PostDynamicTypedObject));

        var dbContextPutMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.PutDynamicTypedObject));

        var dbContextPatchMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.PatchDynamicTypedObject));

        var dbContextDeleteMethod = methods.First(m => m.Name == nameof(IDynamicDbContext.DeleteDynamicTypedObject));

        foreach (var (entityName, (entity, typeBuilder)) in GetTablesAndTypeBuilders())
        {
            var t = typeBuilder.CreateType();
            var pluralName = entityName.Pluralize();

            if (entity.Key.IsComposite)
            {
                // add complex type to handle composite keys
                builder.AddComplexType(t);
            }
            else
            {
                var entityType = builder.AddEntityType(t);
                builder.AddEntitySet(pluralName, entityType);
            }

            _dynamicDbEntities[pluralName] = new DynamicDbEntity
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
                DbContextPostMethod = dbContextPostMethod.MakeGenericMethod(t!),
                DbContextPutMethod = dbContextPutMethod.MakeGenericMethod(t!),
                DbContextPatchMethod = dbContextPatchMethod.MakeGenericMethod(t!),
                DbContextDeleteMethod = dbContextDeleteMethod.MakeGenericMethod(t!)
            };
        }

        _edmModel = builder.GetEdmModel();

        // Check database

        var dbContext = new DynamicDbContext(this);

        var model = dbContext.Model;

        _dynamicService.EnsureDatabaseCreatedIfAutoMigrationsIsSet(dbContext);
    }

    public IDataProvider GetDatabaseProvider() => _databaseProvider;

    public Dictionary<string, IDynamicDbEntity> DynamicDbEntities => _dynamicDbEntities;

    public IDynamicService GetDynamicService() => _dynamicService;

    public ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder)
    {
        foreach (var (key, entity) in _dynamicDbEntities)
        {
            modelBuilder.Entity(entity.Type, b =>
            {
                _databaseProvider.ConfigureEntityTypeBuilder(b, entity.Entity.Table, entity.Entity.Schema);

                var key = entity.Entity.Key;
                if (key.IsComposite)
                {
                    // Create composite key
                    b.HasKey(key.Entities.Select(e => $"{e}Id").ToArray());
                }
                else
                {
                    var prop = SetAttribute(b, key);

                    b.HasKey(new string[] { key.Name });

                    if (!key.IsAutoNumber)
                    {
                        prop.ValueGeneratedNever();
                    }
                }

                foreach (var attr in entity.Entity.Attributes)
                {
                    SetAttribute(b, attr);
                }

                // Set relationship properties
                foreach (var relation in entity.Entity.AllRelationships)
                {
                    if (!relation.IsMany)
                    {
                        var relationProperty = b.Property($"{relation.Name}Id");
                        SetIsRequired(relationProperty, relation.IsRequired);
                    }
                }
            });
        }

        _dynamicService.AddMetadata(modelBuilder);

        SetCascadeBehaviour(modelBuilder);

        return modelBuilder;
    }

    private static void SetCascadeBehaviour(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model
                    .GetEntityTypes()
                    .ToList();

        // Disable cascade delete
        var foreignKeys = entityTypes
            .SelectMany(e => e.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade));

        foreach (var foreignKey in foreignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder SetAttribute(EntityTypeBuilder builder, IBaseEntityAttribute attr)
    {
        var prop = builder.Property(attr.Name);

        var netType = attr.NetDataType();

        prop.HasColumnType(_databaseProvider.ToDatabaseColumnType(attr));

        if (netType == typeof(string))
        {
            prop.HasMaxLength(attr.MaxWidth);
        }

        else if (attr is EntityAttribute attribute && attribute.IsDateTimeType())
        {
            // don't set MaxWidth, throw's error on db create
        }

        else if (attr.MaxWidth > 0 && attr.Precision > 0)
        {
            prop.HasPrecision(attr.MaxWidth, attr.Precision);
        }

        SetIsRequired(prop, attr.IsRequired);

        if (attr.IsUnicode)
        {
            prop.IsUnicode();
        }

        if (attr.MinWidth == attr.MaxWidth)
        {
            prop.IsFixedLength();
        }

        return prop;
    }

    private static void SetIsRequired(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder prop, bool isRequired)
    {
        try
        {
            prop.IsRequired(isRequired);
        }
        catch
        {
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

    public object PutDynamicObject(DbContext context, string dbSetName, string json)
    {
        var parameters = new object[] { json };
        var ret = _dynamicDbEntities[dbSetName].DbContextPutMethod.Invoke(context, parameters);
        return ret!;
    }

    public object PatchDynamicObject(DbContext context, string dbSetName, object id, string json)
    {
        var parameters = new object[] { id, json };
        var ret = _dynamicDbEntities[dbSetName].DbContextPatchMethod.Invoke(context, parameters);
        return ret!;
    }

    public void DeleteDynamicObject(DbContext context, string dbSetName, object id)
    {
        var parameters = new object[] { id };

        _dynamicDbEntities[dbSetName].DbContextDeleteMethod.Invoke(context, parameters);
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

            foreach (var relation in entity.AllRelationships)
            {
                var relatedEntity = dynamicTypes[relation.Entity];
                var relatedTb = relatedEntity.TypeBuilder;

                if (relation.IsMany)
                {
                    tb.AddPublicGetSetPropertyAsList(relation.Name, relatedTb);
                }
                else
                {
                    var property = tb.AddPublicGetSetProperty(relation.Name, relatedTb);
                }
            }

            if (entity.Key.IsComposite)
            {
                // Add properties for each composite key entity
                foreach (var keyEntity in entity.Key.Entities)
                {
                    var relatedTb = dynamicTypes[keyEntity].TypeBuilder;
                    tb.AddPublicGetSetProperty(keyEntity, relatedTb);
                }
            }
            else
            {
                // Add simple key
                tb.AddPublicGetSetProperty(entity.Key.Name, entity.Key.NetDataType());
            }
        }

        return dynamicTypes;
    }
}