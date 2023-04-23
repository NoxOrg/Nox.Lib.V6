using Nox.Core.Interfaces.Entity;

namespace Nox.Ui.Data
{
    internal interface INoxDataService
    {
        Task<NoxDataGrid> Find(IEntity entity, int skip = 0, int top = 10, 
            string? orderby = null, bool? desc = null,
            string? filter = null
        );
    }
}