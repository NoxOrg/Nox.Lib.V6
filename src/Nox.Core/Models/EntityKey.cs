using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Core.Models;

public sealed class EntityKey : BaseEntityAttribute
{
    public override bool IsRequired { get; set; } = true;

    public bool IsAutoNumber { get; set; } = false;

    [NotMapped]
    public string[] Entities { get; set; } = Array.Empty<string>();

    public bool IsComposite { get => Entities.Any(); }
}