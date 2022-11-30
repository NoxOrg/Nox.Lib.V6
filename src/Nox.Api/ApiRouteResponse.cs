using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Api
{
    public sealed class ApiRouteResponse : MetaBase, IApiRouteResponse
    {
        public string Type { get; set; } = "string";
        public bool IsCollection { get; set; } = false;

    }
}
