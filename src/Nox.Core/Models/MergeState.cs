using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Core.Models
{
    public sealed class MergeState : MetaBase
    {
        public string Loader { get; set; } = String.Empty;
        public string Property { get; set; } = String.Empty;
        public DateTime LastDateLoadedUtc { get; set; }
            
    }
}
