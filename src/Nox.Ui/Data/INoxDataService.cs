using Nox.Core.Interfaces.Entity;

namespace Nox.Ui.Data
{
    internal interface INoxDataService
    {
        Task<IEnumerable<dynamic>> Find(IEntity entity, int skip = 0, int top = 10);
    }
}