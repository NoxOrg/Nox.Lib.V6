using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    public class ApiRoute
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HttpVerb { get; set; } = "GET";
        public List<ApiRouteParameter> Parameters { get; set; } = new();
        public List<ApiRouteResponse> Responses { get; set; } = new();
        public string TargetUrl { get; set; } = null!;
    }
}
