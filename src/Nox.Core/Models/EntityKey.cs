using Nox.Core.Interfaces.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Core.Models;

public sealed class EntityKey : BaseEntityAttribute, IEntityKey
{
    public override bool IsRequired { get => true; }

    [NotMapped]
    public string[] Entities { get; set; } = Array.Empty<string>();

    public bool IsComposite { get => Entities.Any(); }
}