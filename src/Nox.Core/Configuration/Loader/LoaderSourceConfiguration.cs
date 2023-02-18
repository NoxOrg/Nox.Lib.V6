using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class LoaderSourceConfiguration: MetaBase 
{
    public string DataSource { get; set; } = string.Empty; 
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}