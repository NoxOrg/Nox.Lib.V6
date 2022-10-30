using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using System.Text.Json;

namespace Nox.Data;

public interface IDynamicModel
{
    ModelBuilder ConfigureDbContextModel(ModelBuilder modelBuilder);
    IDatabaseProvider GetDatabaseProvider();
    IQueryable GetDynamicCollection(DbContext context, string dbSetName);
    object GetDynamicNavigation(DbContext context, string dbSetName, object id, string navName);
    object GetDynamicObjectProperty(DbContext context, string dbSetName, object id, string propName);
    object GetDynamicSingleResult(DbContext context, string dbSetName, object id);
    IEdmModel GetEdmModel();
    object PostDynamicObject(DbContext context, string dbSetName, string obj);
    void SetupRecurringLoaderTasks();
}
