namespace Nox.Core.Interfaces.Etl;

public interface IEtlTargetMessageQueue: IMetaBase
{
    string Name { get; set; }
    string Provider { get; set; }
    string ConnectionString { get; set; }
    string Queue { get; set; }
    IEtlRetryPolicy? Retry { get; set; }
}