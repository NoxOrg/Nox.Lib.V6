namespace Nox.Core.Interfaces.Etl;

public interface IEtlTargets
{
    ICollection<IEtlMessageQueue>? MessageQueues { get; set; }
    ICollection<IEtlFile>? Files { get; set; }
    ICollection<IEtlTargetDatabase>? Databases { get; set; }
    ICollection<IEtlHttp>? Http { get; set; }
}