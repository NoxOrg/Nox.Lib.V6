using Nox.Core.Components;
using Nox.Core.Interfaces.Api;

namespace Nox.Api
{
    public sealed class ApiRouteParameter : MetaBase, IApiRouteParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "string";
        public object? Default { get; set; }
        public int MinValue { get; set; } = int.MinValue;
        public int MaxValue { get; set; } = int.MaxValue;
    }
}
