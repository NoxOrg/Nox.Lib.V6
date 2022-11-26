using Nox.Core.Components;

namespace Nox.Core.Models
{
    public sealed class ApiRouteResponse : MetaBase
    {
        public string Type { get; set; } = "string";
        public bool IsCollection { get; set; } = false;

    }
}
