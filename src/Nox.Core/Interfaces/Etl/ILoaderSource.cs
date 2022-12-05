namespace Nox.Core.Interfaces;

public interface ILoaderSource: IMetaBase
{
    string Name { get; set; }
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
}