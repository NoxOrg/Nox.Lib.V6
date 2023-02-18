using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlMessageQueue: MetaBase, IEtlMessageQueue
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    
    IEtlRetryPolicy? IEtlMessageQueue.Retry
    {
        get => Retry;
        set => Retry = value as EtlRetryPolicy;
    }
        
    public EtlRetryPolicy? Retry { get; set; }
    
}