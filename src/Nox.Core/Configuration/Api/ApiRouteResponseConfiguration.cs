using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ApiRouteResponseConfiguration: MetaBase
{
    public string? Type { get; set; }
    public bool? IsCollection { get; set; }
}