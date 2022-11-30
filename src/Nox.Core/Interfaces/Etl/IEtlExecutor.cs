using Nox.Core.Interfaces.Database;

namespace Nox.Core.Interfaces.Etl
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(IMetaService service);
        Task<bool> ExecuteLoaderAsync(ILoader loader, IDatabaseProvider destinationDbProvider, IEntity entity);
    }
}