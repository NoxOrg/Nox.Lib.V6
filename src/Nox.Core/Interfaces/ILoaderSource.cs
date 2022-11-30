namespace Nox.Core.Interfaces;

public interface ILoaderSource: IServiceDatabase
{
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
}