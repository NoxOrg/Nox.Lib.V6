namespace Nox.Core.Interfaces;

public interface IApiRoute: IMetaBase
{
    string Name { get; set; } 
    string Description { get; set; } 
    string HttpVerb { get; set; }
    ICollection<IApiRouteParameter>? Parameters { get; set; }
    ICollection<IApiRouteResponse>? Responses { get; set; }
    string TargetUrl { get; set; } 
}