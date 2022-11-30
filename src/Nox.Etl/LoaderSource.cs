using Nox.Core.Interfaces;
using Nox.Core.Models;

namespace Nox.Etl;

public sealed class LoaderSource : DatabaseBase, ILoaderSource
{
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}


