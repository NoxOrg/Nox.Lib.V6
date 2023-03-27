using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityAttributeConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsAutoNumber { get; set; } = false;
    public bool IsForeignKey { get; set; } = false;
    public bool IsRequired { get; set; } = false;
    public bool IsUnicode { get; set; } = true;
    public bool IsTemporalOnly { get; set; } = false;
    public bool CanFilter { get; set; } = false;
    public bool CanSort { get; set; } = false;
    public int MinWidth { get; set; } = 0;
    public int MaxWidth { get; set; } = 512;
    public int MinValue { get; set; } = int.MinValue;
    public int MaxValue { get; set; } = int.MaxValue;
    public int Precision { get; set; } = 2;
    public object? Default { get; set; }
    public string Formula { get; set; } = string.Empty;
}