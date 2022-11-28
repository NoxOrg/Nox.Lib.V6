using Nox.Core.Components;
using Nox.Core.Models;

namespace Nox.Core.Interfaces
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(MetaService service);
        Task<bool> ExecuteLoaderAsync(Loader loader, IDatabaseProvider destinationDbProvider, Entity entity);
    }
}