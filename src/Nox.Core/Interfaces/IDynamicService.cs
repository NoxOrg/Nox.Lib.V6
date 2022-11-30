using Microsoft.EntityFrameworkCore;

namespace Nox.Core.Interfaces
{
    public interface IDynamicService
    {
        string Name { get; }
        IMetaService MetaService { get; }
        string KeyVaultUri { get; }
        IReadOnlyDictionary<string, IApi>? Apis { get; }
        IReadOnlyDictionary<string, IEntity>? Entities { get; }
        IEnumerable<ILoader>? Loaders { get; }
        Task<bool> ExecuteDataLoaderAsync(ILoader loader, IDatabaseProvider destinationDbProvider);
        Task<bool> ExecuteDataLoadersAsync();
        void AddMetadata(ModelBuilder modelBuilder);
        void SetupRecurringLoaderTasks();
        void EnsureDatabaseCreated(DbContext dbContext);
    }
}