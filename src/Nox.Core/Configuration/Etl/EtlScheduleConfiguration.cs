using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlScheduleConfiguration: MetaBase
{
    public string Start { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public EtlRetryPolicyConfiguration? Retry { get; set; } = new();
}