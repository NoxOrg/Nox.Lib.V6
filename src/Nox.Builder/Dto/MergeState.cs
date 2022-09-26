using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class MergeState
    {
        public string Loader { get; set; } = String.Empty;
        public string Property { get; set; } = String.Empty;
        public DateTime LastDateLoaded { get; set; }
            
    }
}
