using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public sealed class LoaderSource: MetaBase, ILoaderSource
{
    public string Name { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}


