using Nox.Core.Components;

namespace Nox.Core.Models.Entity
{
    public class QueryResponse : MetaBase
    {
        public string ResponseDto { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsMany { get; set; } = false;
    }
}
