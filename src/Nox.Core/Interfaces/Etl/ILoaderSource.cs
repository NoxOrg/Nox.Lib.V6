using Nox.Core.Interfaces.Database;

namespace Nox.Core.Interfaces.Etl;

public interface ILoaderSource: IServiceDatabase
{
    string Query { get; set; }
    int MinimumExpectedRecords { get; set; }
}