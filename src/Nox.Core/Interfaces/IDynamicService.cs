using Microsoft.EntityFrameworkCore;
using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Interfaces
{
    public interface IDynamicService
    {
        string Name { get; }
        string KeyVaultUri { get; }
        bool AutoMigrations { get; }
        IMetaService MetaService { get; }
        IReadOnlyDictionary<string, IApi>? Apis { get; }
        IReadOnlyDictionary<string, IEntity>? Entities { get; }
        IEnumerable<ILoader>? Loaders { get; }
        Task<bool> ExecuteDataLoaderAsync(ILoader loader);
        Task<bool> ExecuteDataLoadersAsync();
        void AddMetadata(ModelBuilder modelBuilder);
        void SetupRecurringLoaderTasks();
        void EnsureDatabaseCreatedIfAutoMigrationsIsSet(DbContext dbContext);
    }
}