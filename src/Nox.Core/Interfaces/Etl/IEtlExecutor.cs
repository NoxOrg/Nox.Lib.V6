using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Solution;

namespace Nox.Core.Interfaces.Etl
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(IDataProvider entityStore);
        Task<bool> ExecuteEtlAsync(Integration etl, IDataProvider entityStore);
    }
}