using Nox.Data;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.MetaData;

namespace Nox.Dynamic.Services
{
    public interface IDynamicService
    {
        string Name { get; }
        Service Service { get; }
        IServiceDatabase ServiceDatabase { get; }
        string KeyVaultUri { get; }

        IReadOnlyDictionary<string, Api> Apis { get; }
        IReadOnlyDictionary<string, Entity> Entities { get; }
        IReadOnlyCollection<Loader> Loaders { get; }

        Task<bool> ExecuteDataLoaderAsync(Loader loader, IDatabaseProvider destinationDbProvider);
        Task<bool> ExecuteDataLoadersAsync();
    }
}