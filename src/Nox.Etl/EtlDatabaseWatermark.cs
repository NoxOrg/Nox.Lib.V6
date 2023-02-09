using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlDatabaseWatermark: IEtlDatabaseWatermark
{
    public string[] DateColumns { get; set; }
    public string SequentialKeyColumn { get; set; }
}