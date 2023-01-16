using System.Dynamic;
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
        return _dynamicDbModel.PostDynamicObject(this, dbSetName, json);
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

        SaveChanges();

        SendChangeEvent(tObj!.GetType().Name, NoxEventType.Created, json)
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    throw t.Exception;
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        
        return tObj!;
    }

    private async Task SendChangeEvent(string entityName, NoxEventType eventType, string json)
    {
        if (_messenger != null && _messages != null && !string.IsNullOrEmpty(json))
        {
            var dynamicEntity = ((DynamicModel)_dynamicDbModel).DynamicDbEntities.FirstOrDefault(e => e.Value.Entity.Name == entityName);
            if (dynamicEntity.Value != null)
            {
                var msg = _messages.FindEventImplementation(entityName, eventType);
                if (msg != null)
                {
                    var toSend = msg.MapInstance(json, NoxEventSource.NoxDbContext);
                    if (dynamicEntity.Value.Entity.Messaging != null)
                    {
                        await _messenger!.SendMessage(dynamicEntity.Value.Entity.Messaging, toSend);    
                    }
                }
            }
        }
    }
}