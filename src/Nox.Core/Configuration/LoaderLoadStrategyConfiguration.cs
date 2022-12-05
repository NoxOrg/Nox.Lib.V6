namespace Nox.Core.Configuration;

public class LoaderLoadStrategyConfiguration
{
    public string Type { get; set; } = string.Empty;
    public string[] Columns { get; set; } = Array.Empty<string>();
}