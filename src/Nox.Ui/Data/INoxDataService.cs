using Nox.Solution;

namespace Nox.Ui.Data
{
    internal interface INoxDataService
    {
        Task<IEnumerable<dynamic>> Find(Entity entity, int skip = 0, int top = 10);
    }
}