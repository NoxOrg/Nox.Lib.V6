namespace Nox.Core.Interfaces
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(IMetaService service);
        Task<bool> ExecuteLoaderAsync(ILoader loader, IDatabaseProvider destinationDbProvider, IEntity entity);
    }
}