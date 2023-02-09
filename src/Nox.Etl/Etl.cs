using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class Etl: MetaBase, IEtl
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string TargetType { get; set; }
    
    IEtlSources? IEtl.Sources
    {
        get => Sources;
        set => Sources = value as EtlSources;
    }
    public EtlSources? Sources { get; set; }
    
    IEtlTargets? IEtl.Targets
    {
        get => Targets;
        set => Targets = value as EtlTargets;
    }

    public EtlTargets? Targets { get; set; }
    
}


