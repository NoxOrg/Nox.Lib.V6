using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityKeyConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsAutoNumber { get; set; } = false;
    public bool IsUnicode { get; set; } = true;
    public int MinWidth { get; set; } = 0;
    public int MaxWidth { get; set; } = 512;
    public List<string> Entities { get; set; } = new();
}