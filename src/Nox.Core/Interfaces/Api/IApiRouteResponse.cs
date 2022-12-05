namespace Nox.Core.Interfaces;

public interface IApiRouteResponse: IMetaBase
{
    string Type { get; set; }
    bool IsCollection { get; set; }
}