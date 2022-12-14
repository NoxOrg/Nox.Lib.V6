using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Etl
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(IMetaService service);
        Task<bool> ExecuteLoaderAsync(IMetaService service, ILoader loader, IEntity entity);
    }
}