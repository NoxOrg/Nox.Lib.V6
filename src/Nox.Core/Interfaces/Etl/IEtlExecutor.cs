using Nox.Core.Interfaces.Entity;
using Nox.Solution;

namespace Nox.Core.Interfaces.Etl
{
    public interface IEtlExecutor
    {
        Task<bool> ExecuteAsync(NoxSolution solution);
        Task<bool> ExecuteEtlAsync(NoxSolution solution, Integration etl, Solution.Entity entity);
    }
}