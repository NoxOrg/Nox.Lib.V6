using Nox.Core.Components;

namespace Nox.Core.Models.Entity
{
    public class EntityCommand : MetaBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
