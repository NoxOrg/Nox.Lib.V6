namespace Nox.Core.Interfaces.Etl;

public interface IEtlTransform
{
    ICollection<IEtlMap>? Map { get; set; }
    ICollection<IEtlLookup>? Lookups { get; set; }
}