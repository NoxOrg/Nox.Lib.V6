using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ApiRouteConfiguration: MetaBase
{
    public string? Name { get; set; } 
    public string? Description { get; set; } 
    public string? HttpVerb { get; set; }
    public List<ApiRouteParameterConfiguration>? Parameters { get; set; }
    public List<ApiRouteResponseConfiguration>? Responses { get; set; }
    public string? TargetUrl { get; set; } 
}