using Nox.Core.Models;

namespace Nox.Core.Components;

public sealed class LoaderSource : DatabaseBase
{
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}


