using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlSources: MetaBase, IEtlSources
{
    ICollection<IEtlTargetMessageQueue>? IEtlSources.MessageQueues
    {
        get => MessageQueues?.ToList<IEtlTargetMessageQueue>();
        set => MessageQueues = value as ICollection<EtlTargetMessageQueue>;
    }

    public ICollection<EtlTargetMessageQueue>? MessageQueues { get; set; }
    
    ICollection<IEtlSourceFile>? IEtlSources.Files
    {
        get => Files?.ToList<IEtlSourceFile>();
        set => Files = value as ICollection<EtlSourceFile>;
    }

    public ICollection<EtlSourceFile>? Files { get; set; }
    
    ICollection<IEtlSourceDatabase>? IEtlSources.Databases
    {
        get => Databases?.ToList<IEtlSourceDatabase>();
        set => Databases = value as ICollection<EtlSourceDatabase>;
    }
    
    public ICollection<EtlSourceDatabase>? Databases { get; set; }
    
    ICollection<IEtlSourceHttp>? IEtlSources.Http
    {
        get => Http?.ToList<IEtlSourceHttp>();
        set => Http = value as ICollection<EtlSourceHttp>;
    }
    
    public ICollection<EtlSourceHttp>? Http { get; set; }
}