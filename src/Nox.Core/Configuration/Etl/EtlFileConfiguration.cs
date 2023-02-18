using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlFileConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
    public EtlScheduleConfiguration? Schedule { get; set; } = new();
}