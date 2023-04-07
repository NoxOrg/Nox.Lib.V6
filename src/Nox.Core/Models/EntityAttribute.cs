using Nox.Core.Interfaces.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Core.Models;

public sealed class EntityAttribute : BaseEntityAttribute, IEntityAttribute
{
    public bool IsAutoNumber { get; set; } = false;
    public bool IsForeignKey { get; set; } = false;
    public bool IsTemporalOnly { get; set; } = false;
    public bool CanFilter { get; set; } = false;
    public bool CanSort { get; set; } = false;
    public int MinValue { get; set; } = int.MinValue;
    public int MaxValue { get; set; } = int.MaxValue;
    public object? Default { get; set; }

    [NotMapped]
    public string[] DefaultFromParents { get; set; } = Array.Empty<string>();
    public string DefaultFromParentsJson { get => string.Join('|', DefaultFromParents.ToArray()); set => DefaultFromParents = value.Split('|'); }
    public string Formula { get; set; } = string.Empty;

    public bool IsMappedAttribute() => (string.IsNullOrEmpty(Formula) && !DefaultFromParents.Any());


    public bool ApplyDefaults()
    {
        if (MaxWidth < 1)
            MaxWidth = 512;

        Type = Type.ToLower();

        DefaultFromParents = DefaultFromParents.Where(p => p.Trim().Length > 0).ToArray();

        return true;
    }
}