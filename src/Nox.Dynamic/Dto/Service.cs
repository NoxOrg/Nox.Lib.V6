using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class Service
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceDatabase Database { get; set; } = new();
        public Dictionary<string,Entity> Entities { get; set; } = null!;
        public Dictionary<string,Loader> Loaders { get; set; } = null!;
    }
}
