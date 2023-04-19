using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Api;

namespace Nox.Core.Models
{
    public sealed class ApiRouteResponse : MetaBase, IApiRouteResponse
    {
        public string Type { get; set; } = "string";
        public bool IsCollection { get; set; } = false;

    }
}
