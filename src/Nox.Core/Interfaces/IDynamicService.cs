using Nox.Core.Components;
using Nox.Core.Models;

namespace Nox.Core.Interfaces
{
    public interface IDynamicService
    {
        string Name { get; }
        MetaService Service { get; }
        IServiceDatabase ServiceDatabase { get; }
        string KeyVaultUri { get; }

        IReadOnlyDictionary<string, Api> Apis { get; }
        IReadOnlyDictionary<string, Entity> Entities { get; }
        IReadOnlyCollection<Loader> Loaders { get; }

        Task<bool> ExecuteDataLoaderAsync(Loader loader, IDatabaseProvider destinationDbProvider);
        Task<bool> ExecuteDataLoadersAsync();
    }
}