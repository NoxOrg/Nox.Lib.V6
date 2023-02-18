namespace Nox.Core.Interfaces.Etl;

public interface IEtlSourceDatabase: IMetaBase
{
    string Name { get; set; }
    string ConnectionString { get; set; }
    string Provider { get; set; }
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
    IEtlSchedule? Schedule { get; set; }
    IEtlDatabaseWatermark? Watermark { get; set; }
    IEtlTransform? Transform { get; set; }
    bool InvokeHandler { get; set; }
}