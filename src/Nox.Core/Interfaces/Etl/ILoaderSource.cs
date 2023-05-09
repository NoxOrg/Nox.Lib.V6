namespace Nox.Core.Interfaces.Etl;

public interface ILoaderSource: IMetaBase
{
    string DataSource { get; set; }
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
}