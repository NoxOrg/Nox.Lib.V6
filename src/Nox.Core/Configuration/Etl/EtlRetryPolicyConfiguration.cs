using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EtlRetryPolicyConfiguration: MetaBase
{
    public int Limit { get; set; } = 0;
    public int DelaySeconds { get; set; } = 0;
    public int DoubleDelayLimit { get; set; } = 0;
}