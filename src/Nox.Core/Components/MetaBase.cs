using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Interfaces;

namespace Nox.Core.Components;

public class MetaBase : IMetaBase
{
    public int Id { get; set; }

    [NotMapped]
    public string DefinitionFileName { get; set; } = String.Empty;
}
