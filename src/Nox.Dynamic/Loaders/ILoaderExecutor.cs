using Nox.Data;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.MetaData;

namespace Nox.Dynamic.Loaders
{
    public interface ILoaderExecutor
    {
        Task<bool> ExecuteAsync(Service service);
        Task<bool> ExecuteLoaderAsync(Loader loader, IDatabaseProvider destinationDbProvider, Entity entity);
    }
}