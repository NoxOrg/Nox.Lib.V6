namespace Nox.Core.Interfaces.Etl;

public interface IEtlTargets
{
    ICollection<IEtlTargetMessageQueue>? MessageQueues { get; set; }
    ICollection<IEtlSourceFile>? Files { get; set; }
    ICollection<IEtlTargetDatabase>? Databases { get; set; }
    ICollection<IEtlSourceHttp>? Http { get; set; }
}