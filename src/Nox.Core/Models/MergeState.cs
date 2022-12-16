using Nox.Core.Components;

namespace Nox.Core.Models
{
    public sealed class MergeState : MetaBase
    {
        public string Loader { get; set; } = String.Empty;
        public string Property { get; set; } = String.Empty;
        public DateTime LastDateLoadedUtc { get; set; }
        public bool Updated { get; set; } = false;
            
    }
}
