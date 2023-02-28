using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlTargets: MetaBase, IEtlTargets
{
    ICollection<IEtlTargetMessageQueue>? IEtlTargets.MessageQueues
    {
        get => MessageQueues?.ToList<IEtlTargetMessageQueue>();
        set => MessageQueues = value as ICollection<EtlTargetMessageQueue>;
    }

    public ICollection<EtlTargetMessageQueue>? MessageQueues { get; set; }
    
    ICollection<IEtlSourceFile>? IEtlTargets.Files
    {
        get => Files?.ToList<IEtlSourceFile>();
        set => Files = value as ICollection<EtlSourceFile>;
    }

    public ICollection<EtlSourceFile>? Files { get; set; }
    
    ICollection<IEtlTargetDatabase>? IEtlTargets.Databases
    {
        get => Databases?.ToList<IEtlTargetDatabase>();
        set => Databases = value as ICollection<EtlTargetDatabase>;
    }
    
    public ICollection<EtlTargetDatabase>? Databases { get; set; }
    
    ICollection<IEtlSourceHttp>? IEtlTargets.Http
    {
        get => Http?.ToList<IEtlSourceHttp>();
        set => Http = value as ICollection<EtlSourceHttp>;
    }
    
    public ICollection<EtlSourceHttp>? Http { get; set; }
}