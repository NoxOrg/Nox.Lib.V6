using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Interfaces.Etl
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(IProjectConfiguration service);
        Task<bool> ExecuteLoaderAsync(IProjectConfiguration service, ILoader loader, IEntity entity);
    }
}