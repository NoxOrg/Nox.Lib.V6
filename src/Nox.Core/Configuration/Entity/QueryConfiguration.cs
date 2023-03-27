using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class QueryConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;    

    public List<ParameterConfiguration> Parameters { get; set; } = new();

    public ResponseConfiguration Response { get; set; } = new();
}