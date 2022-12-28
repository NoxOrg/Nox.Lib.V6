using System.Text.Json;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Nox.Core.Enumerations;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Messaging.Events;
using Nox.Messaging;

namespace Nox.Data;

public class DynamicDbContext : DbContext, IDynamicDbContext
{
    private readonly IDynamicModel _dynamicDbModel;
    private IEnumerable<INoxEvent>? _messages = null;
    private readonly INoxMessenger? _messenger = null;

    public DynamicDbContext(
        DbContextOptions<DynamicDbContext> options,
        IDynamicModel dynamicDbModel,
        IEnumerable<INoxEvent>? messages = null,
        INoxMessenger? messenger = null)
        : base(options)
    {
        _dynamicDbModel = dynamicDbModel;
        _messenger = messenger;
        _messages = messages;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DynamicDbContext(IDynamicModel dynamicDbModel)
    {
        _dynamicDbModel = dynamicDbModel;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var provider = _dynamicDbModel.GetDatabaseProvider();

            provider.ConfigureDbContext(optionsBuilder);
        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _dynamicDbModel.ConfigureDbContextModel(modelBuilder);
    }

    // Methods for Controller access
    public IQueryable GetDynamicCollection(string dbSetName)
    {
        return _dynamicDbModel.GetDynamicCollection(this, dbSetName);
    }

    public object GetDynamicSingleResult(string dbSetName, object id)
    {
        return _dynamicDbModel.GetDynamicSingleResult(this, dbSetName, id);
    }

    public object GetDynamicObjectProperty(string dbSetName, object id, string propName)
    {
        return _dynamicDbModel.GetDynamicObjectProperty(this, dbSetName, id, propName);
    }

    public object GetDynamicNavigation(string dbSetName, object id, string navName)
    {
        return _dynamicDbModel.GetDynamicNavigation(this, dbSetName, id, navName);
    }

    public object PostDynamicObject(string dbSetName, string json)
    {
        var result = _dynamicDbModel.PostDynamicObject(this, dbSetName, json);
         
        if (_messenger != null && _messages != null && result != null)
        {
            var dynamicEntity = ((DynamicModel)_dynamicDbModel).DynamicDbEntities.FirstOrDefault(e => e.Value.Entity.PluralName == dbSetName).Value;
            if (dynamicEntity != null)
            {
                var msg = _messages.FindEventImplementation(dynamicEntity.Entity.Name, NoxEventTypeEnum.Create);
                if (msg != null)
                {
                    SendChangeEvent(dynamicEntity.Entity, result, msg, NoxEventSourceEnum.NoxEventSource_DbContext);
                }
            }
            
        }

        return result;
    }

    // Strongly typed methods for model callback

    public IQueryable<T> GetDynamicTypedCollection<T>() where T : class
    {
        return Set<T>();
    }

    public object GetDynamicTypedSingleResult<T>(object id) where T : class
    {
        var collection = GetDynamicTypedCollection<T>();

        var whereLambda = id.GetByIdExpression<T>();

        var result = collection.Where(whereLambda);

        var obj = SingleResult.Create(result)!;

        return obj;
    }

    public object GetDynamicTypedObjectProperty<T>(object id, string propName) where T : class
    {
        var whereResult = GetDynamicTypedSingleResult<T>(id) as SingleResult<T>;

        var selectPropertyExpression = propName.GetPropertyValueExpression<T>();

        var result = whereResult!.Queryable.Select(selectPropertyExpression);

        return result.Single();
    }

    public object GetDynamicTypedNavigation<T>(object id, string navName) where T : class
    {
        var whereResult = GetDynamicTypedSingleResult<T>(id) as SingleResult<T>;

        var selectPropertyExpression = navName.GetPropertyValueExpression<T>();

        var result = whereResult!.Queryable.Include(navName).Select(selectPropertyExpression);

        return result.Single();
    }

    public object PostDynamicTypedObject<T>(string json) where T : class
    {
        var repo = Set<T>();

        var tObj = JsonSerializer.Deserialize<T>(json);

        repo.Add(tObj!);

        this.SaveChanges();
        if (_messenger != null && _messages != null && tObj != null)
        {
            var dynamicEntity = ((DynamicModel)_dynamicDbModel).DynamicDbEntities.FirstOrDefault(e => e.Value.Entity.PluralName == tObj.GetType().Name).Value;
            if (dynamicEntity != null)
            {
                var msg = _messages.FindEventImplementation(dynamicEntity.Entity.Name, NoxEventTypeEnum.Create);
                if (msg != null)
                {
                    SendChangeEvent(dynamicEntity.Entity, tObj, msg, NoxEventSourceEnum.NoxEventSource_DbContext);
                }
            }
            
        }


        return tObj!;

    }
    
    private void SendChangeEvent(IEntity entity, Object obj, INoxEvent message, NoxEventSourceEnum eventSource)
    {
        var toSend = message.MapInstance(obj, eventSource);
        _messenger?.SendMessage(entity, toSend);
    }
}