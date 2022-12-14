namespace Nox.Core.Interfaces.Etl;

public interface ILoaderSource: IMetaBase
{
    string Name { get; set; }
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
}