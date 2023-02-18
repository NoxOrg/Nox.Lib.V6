using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class ApiConfiguration: MetaBase
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<ApiRouteConfiguration>? Routes { get; set; }
}