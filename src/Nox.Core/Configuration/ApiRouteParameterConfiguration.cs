using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ApiRouteParameterConfiguration: MetaBase
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public object? Default { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
}