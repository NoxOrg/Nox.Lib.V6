using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
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
