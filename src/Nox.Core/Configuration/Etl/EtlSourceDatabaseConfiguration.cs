using Nox.Core.Components;
using Nox.Core.Interfaces.Configuration;

namespace Nox.Core.Configuration;

public class EtlSourceDatabaseConfiguration: MetaBase, IEtlSourceConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
    public EtlScheduleConfiguration? Schedule { get; set; } = new();
    public EtlDatabaseWatermarkConfiguration? Watermark { get; set; } = new();
    public EtlTransformConfiguration? Transform { get; set; } = new();
    public bool InvokeHandler { get; set; } = false;
}