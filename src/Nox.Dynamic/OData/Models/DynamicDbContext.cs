using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nox.Dynamic.Extensions;
using System.Text.Json;

namespace Nox.Dynamic.OData.Models
{
    public partial class DynamicDbContext : DbContext
    {
        private readonly DynamicDbModel _dynamicDbModel;

        public DynamicDbContext(
            DbContextOptions<DynamicDbContext> options,
            DynamicDbModel dynamicDbModel
        )
            : base(options)
        {
            _dynamicDbModel = dynamicDbModel;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_dynamicDbModel.GetDatabaseConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _dynamicDbModel?.ConfigureDbContextModel(modelBuilder);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        // Methods for Controller access
        public IQueryable GetDynamicCollection(string dbSetName)
        {
            return _dynamicDbModel?.GetDynamicCollection(this, dbSetName)!;
        }

        public object GetDynamicSingleResult(string dbSetName, object id)
        {
            return _dynamicDbModel?.GetDynamicSingleResult(this, dbSetName, id)!;
        }

        public object GetDynamicObjectProperty(string dbSetName, object id, string propName)
        {
            return _dynamicDbModel?.GetDynamicObjectProperty(this, dbSetName, id, propName)!;
        }

        public object GetDynamicNavigation(string dbSetName, object id, string navName)
        {
            return _dynamicDbModel?.GetDynamicNavigation(this, dbSetName, id, navName)!;
        }

        public object PostDynamicObject(string dbSetName, JsonElement obj)
        {
            return _dynamicDbModel?.PostDynamicObject(this, dbSetName, obj)!;
        }

        // Stronly typed methods for model callback

        public IQueryable<T> GetDynamicTypedCollection<T>() where T : class
        {
            return Set<T>();
        }

        public SingleResult<T> GetDynamicTypedSingleResult<T>(object id) where T : class
        {
            var collection = GetDynamicTypedCollection<T>();

            var whereLambda = id.GetByIdExpression<T>();

            var result = collection.Where(whereLambda);

            return SingleResult.Create(result);
        }

        public object GetDynamicTypedObjectProperty<T>(object id, string propName) where T : class
        {
            var whereResult = GetDynamicTypedSingleResult<T>(id);

            var selectPropertyExpression = propName.GetPropertyValueExpression<T>();

            var result = whereResult.Queryable.Select(selectPropertyExpression);

            return result.Single();
        }

        public object GetDynamicTypedNavigation<T>(object id, string navName) where T : class
        {
            var whereResult = GetDynamicTypedSingleResult<T>(id);

            var selectPropertyExpression = navName.GetPropertyValueExpression<T>();

            var result = whereResult.Queryable.Include(navName).Select(selectPropertyExpression);

            return result.Single();
        }

        public object PostDynamicTypedObject<T>(JsonElement obj) where T : class
        {
            var repo = Set<T>();
            
            var json = obj.GetRawText();

            var tObj = JsonSerializer.Deserialize<T>(json);

            repo.Add(tObj!);

            this.SaveChanges();

            return tObj!;
        }
    }

}
