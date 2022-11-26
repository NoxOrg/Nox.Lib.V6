using System.Collections.ObjectModel;
using Nox.Core.Components;

namespace Nox.Core.Models
{
    public sealed class ApiRoute : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HttpVerb { get; set; } = "GET";
        public ICollection<ApiRouteParameter> Parameters { get; set; } = new Collection<ApiRouteParameter>();
        public ICollection<ApiRouteResponse> Responses { get; set; } = new Collection<ApiRouteResponse>();
        public string TargetUrl { get; set; } = null!;
    }
}
