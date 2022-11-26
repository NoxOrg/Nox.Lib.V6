using Nox.Core.Components;

namespace Nox.Core.Models
{
    public sealed class ApiRouteParameter : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "string";
        public object? Default { get; set; }
        public int MinValue { get; set; } = int.MinValue;
        public int MaxValue { get; set; } = int.MaxValue;
    }
}
