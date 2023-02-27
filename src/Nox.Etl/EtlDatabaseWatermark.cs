using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlDatabaseWatermark: MetaBase, IEtlDatabaseWatermark
{
    [NotMapped]
    public string[] DateColumns { get; set; } = null!;
    public string SequentialKeyColumn { get; set; } = string.Empty;
}