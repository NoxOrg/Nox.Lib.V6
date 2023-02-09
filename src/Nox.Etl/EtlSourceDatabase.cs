using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlSourceDatabase: MetaBase, IEtlSourceDatabase
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string Provider { get; set; }
    public string Query { get; set; }
    public int MinimumExpectedRecords { get; set; }
    
    IEtlSchedule? IEtlSourceDatabase.Schedule
    {
        get => Schedule;
        set => Schedule = value as EtlSchedule;
    }
    
    public EtlSchedule? Schedule { get; set; }
    
    IEtlDatabaseWatermark? IEtlSourceDatabase.Watermark
    {
        get => Watermark;
        set => Watermark = value as EtlDatabaseWatermark;
    }
        
    public EtlDatabaseWatermark? Watermark { get; set; }
    
    IEtlTransform? IEtlSourceDatabase.Transform
    {
        get => Transform;
        set => Transform = value as EtlTransform;
    }
    
    public EtlTransform? Transform { get; set; }
    
    public bool InvokeHandler { get; set; }
}