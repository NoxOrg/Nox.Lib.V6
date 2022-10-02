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
        public string KeyVaultUri { get; set; } = "https://we-key-Nox-02.vault.azure.net/";
        public ServiceDatabase Database { get; set; } = new();
        public Dictionary<string,Entity> Entities { get; set; } = null!;
        public Dictionary<string,Loader> Loaders { get; set; } = null!;
        public Dictionary<string,Api> Apis { get; set; } = null!;
    }
}
