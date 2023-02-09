using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlTargets: IEtlTargets
{
    ICollection<IEtlMessageQueue>? IEtlTargets.MessageQueues
    {
        get => MessageQueues?.ToList<IEtlMessageQueue>();
        set => MessageQueues = value as ICollection<EtlMessageQueue>;
    }

    public ICollection<EtlMessageQueue>? MessageQueues { get; set; }
    
    ICollection<IEtlFile>? IEtlTargets.Files
    {
        get => Files?.ToList<IEtlFile>();
        set => Files = value as ICollection<EtlFile>;
    }

    public ICollection<EtlFile>? Files { get; set; }
    
    ICollection<IEtlTargetDatabase>? IEtlTargets.Databases
    {
        get => Databases?.ToList<IEtlTargetDatabase>();
        set => Databases = value as ICollection<EtlTargetDatabase>;
    }
    
    public ICollection<EtlTargetDatabase>? Databases { get; set; }
    
    ICollection<IEtlHttp>? IEtlTargets.Http
    {
        get => Http?.ToList<IEtlHttp>();
        set => Http = value as ICollection<EtlHttp>;
    }
    
    public ICollection<EtlHttp>? Http { get; set; }
}