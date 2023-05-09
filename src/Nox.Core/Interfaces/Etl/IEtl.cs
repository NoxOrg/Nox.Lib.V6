namespace Nox.Core.Interfaces.Etl;

public interface IEtl: IMetaBase
{
    string Name { get; set; }
    string Description { get; set; }
    string TargetType { get; set; }
    IEtlSources? Sources { get; set; }
    IEtlTargets? Targets { get; set; }
}