using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlTransform: MetaBase, IEtlTransform
{
    ICollection<IEtlMap>? IEtlTransform.Map
    {
        get => Map?.ToList<IEtlMap>();
        set => Map = value as ICollection<EtlMap>;
    }

    public ICollection<EtlMap>? Map { get; set; }
    
    ICollection<IEtlLookup>? IEtlTransform.Lookups
    {
        get => Lookups?.ToList<IEtlLookup>();
        set => Lookups = value as ICollection<EtlLookup>;
    }
    
    public ICollection<EtlLookup>? Lookups { get; set; }
}