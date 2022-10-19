using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
{
    public sealed class MergeState : MetaBase
    {
        public string Loader { get; set; } = String.Empty;
        public string Property { get; set; } = String.Empty;
        public DateTime LastDateLoadedUtc { get; set; }
            
    }
}
