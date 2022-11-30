using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Etl;

public sealed class LoaderSource : DatabaseBase, ILoaderSource
{
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}


