namespace Nox.Core.Models;

public sealed class EntityKey : BaseEntityAttribute
{
    public override bool IsRequired { get; set; } = true;

    public bool IsAutoNumber { get; set; } = false;

    public string[] Entities { get; set; } = Array.Empty<string>();

    public bool IsComposite { get => Entities.Any(); }
}