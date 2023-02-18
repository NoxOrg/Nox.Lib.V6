using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlSources: IEtlSources
{
    ICollection<IEtlMessageQueue>? IEtlSources.MessageQueues
    {
        get => MessageQueues?.ToList<IEtlMessageQueue>();
        set => MessageQueues = value as ICollection<EtlMessageQueue>;
    }

    public ICollection<EtlMessageQueue>? MessageQueues { get; set; }
    
    ICollection<IEtlFile>? IEtlSources.Files
    {
        get => Files?.ToList<IEtlFile>();
        set => Files = value as ICollection<EtlFile>;
    }

    public ICollection<EtlFile>? Files { get; set; }
    
    ICollection<IEtlSourceDatabase>? IEtlSources.Databases
    {
        get => Databases?.ToList<IEtlSourceDatabase>();
        set => Databases = value as ICollection<EtlSourceDatabase>;
    }
    
    public ICollection<EtlSourceDatabase>? Databases { get; set; }
    
    ICollection<IEtlHttp>? IEtlSources.Http
    {
        get => Http?.ToList<IEtlHttp>();
        set => Http = value as ICollection<EtlHttp>;
    }
    
    public ICollection<EtlHttp>? Http { get; set; }
}