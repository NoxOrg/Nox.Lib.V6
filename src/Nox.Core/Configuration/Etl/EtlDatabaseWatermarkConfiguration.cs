using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlDatabaseWatermarkConfiguration: MetaBase
{
    public string[] DateColumns { get; set; } = Array.Empty<string>();
    public string SequentialKeyColumn { get; set; } = string.Empty;
}