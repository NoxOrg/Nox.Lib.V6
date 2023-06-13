using Microsoft.EntityFrameworkCore;
using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Solution;

namespace Nox.Core.Interfaces
{
    public interface IDynamicService
    {
        string Name { get; }
        //string KeyVaultUri { get; }
        NoxSolution Solution { get; }
        
        IDataProvider? DatabaseProvider { get; }
        IReadOnlyDictionary<string, Solution.Entity>? Entities { get; }
        IEnumerable<Integration>? Integrations { get; }
        Task<bool> ExecuteIntegrationAsync(Integration integration);
        Task<bool> ExecuteIntegrationsAsync();
        void AddMetadata(ModelBuilder modelBuilder);
        void SetupRecurringIntegrationTasks();
    }
}