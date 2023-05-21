using Nox.Core.Components;

namespace Nox.Core.Models.Entity
{
    public class EntityQuery : MetaBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<QueryParameter> Parameters { get; set; } = new();

        public QueryResponse Response { get; set; } = new();
    }
}
