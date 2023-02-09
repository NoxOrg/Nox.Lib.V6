namespace Nox.Core.Interfaces.Etl;

public interface IEtlRetryPolicy
{
    int Limit { get; set; }
    int DelaySeconds { get; set; }
    int DoubleDelayLimit { get; set; }
}