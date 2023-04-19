using System.ComponentModel.DataAnnotations;
using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Models;

public sealed class LoaderSource : MetaBase, ILoaderSource
{
    public string DataSource { get; set; } = string.Empty;

    [MaxLength(65536)]
    public string Query { get; set; } = string.Empty;

    public int MinimumExpectedRecords { get; set; } = 0;
}