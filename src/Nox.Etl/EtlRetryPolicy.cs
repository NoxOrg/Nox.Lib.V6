using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlRetryPolicy: MetaBase, IEtlRetryPolicy
{
    public int Limit { get; set; }
    public int DelaySeconds { get; set; }
    public int DoubleDelayLimit { get; set; }
}