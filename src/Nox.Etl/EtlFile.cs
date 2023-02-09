using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlFile: MetaBase, IEtlFile
{
    public string Name { get; set; }
    public string Format { get; set; }
    public string Path { get; set; }
    public int MinimumExpectedRecords { get; set; }
    
    IEtlSchedule? IEtlFile.Schedule
    {
        get => Schedule;
        set => Schedule = value as EtlSchedule;
    }
    
    public IEtlSchedule? Schedule { get; set; }
}