namespace Nox.Core.Interfaces.Etl;

public interface IEtlFile: IMetaBase
{
    string Name { get; set; }
    string Format { get; set; }
    string Path { get; set; }
    int MinimumExpectedRecords { get; set; }
    IEtlSchedule? Schedule { get; set; }
}