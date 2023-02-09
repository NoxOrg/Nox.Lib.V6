using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlMessageQueue: MetaBase, IEtlMessageQueue
{
    public string Name { get; set; }
    public string Provider { get; set; }
    public string ConnectionString { get; set; }
    public string Queue { get; set; }
    
    IEtlRetryPolicy? IEtlMessageQueue.Retry
    {
        get => Retry;
        set => Retry = value as EtlRetryPolicy;
    }
        
    public EtlRetryPolicy? Retry { get; set; }
    
}