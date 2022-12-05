
namespace Nox.Core.Interfaces;
 
public interface IDynamicDbContext
{
    IQueryable GetDynamicCollection(string dbSetName);
    object GetDynamicNavigation(string dbSetName, object id, string navName);
    object GetDynamicObjectProperty(string dbSetName, object id, string propName);
    object GetDynamicSingleResult(string dbSetName, object id);
    IQueryable<T> GetDynamicTypedCollection<T>() where T : class;
    object GetDynamicTypedNavigation<T>(object id, string navName) where T : class;
    object GetDynamicTypedObjectProperty<T>(object id, string propName) where T : class;
    object GetDynamicTypedSingleResult<T>(object id) where T : class;
    object PostDynamicObject(string dbSetName, string json);
    object PostDynamicTypedObject<T>(string json) where T : class;
}
