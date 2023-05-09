namespace Nox.Core.Interfaces.Api;

public interface IApiRouteResponse: IMetaBase
{
    string Type { get; set; }
    bool IsCollection { get; set; }
}