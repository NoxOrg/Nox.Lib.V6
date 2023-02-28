namespace Nox.Core.Interfaces.Etl;

public interface IEtlSources
{
    ICollection<IEtlTargetMessageQueue>? MessageQueues { get; set; }
    ICollection<IEtlSourceFile>? Files { get; set; }
    ICollection<IEtlSourceDatabase>? Databases { get; set; }
    ICollection<IEtlSourceHttp>? Http { get; set; }
}