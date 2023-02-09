namespace Nox.Core.Interfaces.Etl;

public interface IEtlTargetDatabase: IMetaBase
{
    string Name { get; set; }
    string ConnectionString { get; set; }
    string Provider { get; set; }
    string StoredProc { get; set; }
}