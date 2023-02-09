using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class LoaderLoadStrategyConfiguration: MetaBase
{
    public string Type { get; set; } = string.Empty;
    public string[] Columns { get; set; } = Array.Empty<string>();
}