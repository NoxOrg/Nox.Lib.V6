namespace Nox.Core.Interfaces.Etl;

public interface IEtlTargetFile: IMetaBase
{
    string Name { get; set; }
    string Format { get; set; }
    string Path { get; set; }
}