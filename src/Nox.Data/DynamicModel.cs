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
using Nox.Solution;

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

        _databaseProvider = dynamicService.DatabaseProvider!;

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

                    b.HasKey(key.Name);

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
                    if (relation.Relationship == RelationshipType.ExactlyOne || relation.Relationship == RelationshipType.ZeroOrOne)
                    {
                        var relationProperty = b.Property($"{relation.Name}Id");
                        SetIsRequired(relationProperty, relation.Relationship == RelationshipType.ExactlyOne);
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

    private Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder SetAttribute(EntityTypeBuilder builder, NoxSimpleTypeDefinition attr)
    {
        var prop = builder.Property(attr.Name);

        var noxType = attr.Type;

        prop.HasColumnType(_databaseProvider.ToDatabaseColumnType(attr));

        if (attr.TextTypeOptions != null)
        {
            prop.HasMaxLength(attr.TextTypeOptions!.MaxLength);
            if (attr.TextTypeOptions.IsUnicode) prop.IsUnicode();
            if (attr.TextTypeOptions.MaxLength == attr.TextTypeOptions.MinLength) prop.IsFixedLength();
        } else if (attr.NumberTypeOptions != null)
        {
            prop.HasPrecision(attr.NumberTypeOptions!.IntegerDigits + attr.NumberTypeOptions.DecimalDigits, attr.NumberTypeOptions.DecimalDigits);
        }
        //todo add code for EntityTypeOptions and MoneyTypeOptions

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
            //Ignore
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

            foreach (var col in entity.Attributes!)
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

                if (relation.Relationship == RelationshipType.ZeroOrMany || relation.Relationship == RelationshipType.OneOrMany)
                {
                    tb.AddPublicGetSetPropertyAsList(relation.Name, relatedTb);
                }
                else
                {
                    tb.AddPublicGetSetProperty(relation.Name, relatedTb);
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