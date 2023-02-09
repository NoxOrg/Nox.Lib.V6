namespace Nox.Core.Interfaces.Etl;

public interface IEtlSources
{
    ICollection<IEtlMessageQueue>? MessageQueues { get; set; }
    ICollection<IEtlFile>? Files { get; set; }
    ICollection<IEtlSourceDatabase>? Databases { get; set; }
    ICollection<IEtlHttp> Http { get; set; }
}