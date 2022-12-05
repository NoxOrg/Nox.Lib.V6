namespace Nox.Core.Interfaces;

public interface IApiRouteParameter : IMetaBase
{
    string Name { get; set; }
    string Type { get; set; }
    object? Default { get; set; }
    int MinValue { get; set; }
    int MaxValue { get; set; }
}