namespace Nox.Core.Interfaces.Etl;

public interface IEtlDatabaseWatermark
{
    string[] DateColumns { get; set; }
    string SequentialKeyColumn { get; set; }
}