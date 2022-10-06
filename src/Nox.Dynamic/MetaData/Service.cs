using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
{
    public sealed class Service : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string KeyVaultUri { get; set; } = "https://we-key-Nox-02.vault.azure.net/";
        public ServiceDatabase Database { get; set; } = new();
        public ICollection<Entity> Entities { get; set; } = null!;
        public ICollection<Loader> Loaders { get; set; } = null!;
        public ICollection<Api> Apis { get; set; } = null!;

        public bool Validate()
        {
            var isValid = true;

            // validate contained definitions

            foreach (var entity in Entities)
            {
                isValid = isValid && entity.Validate();
            }

            foreach (var loader in Loaders)
            {
                isValid = isValid && loader.Validate();
            }

            foreach (var api in Apis)
            {
                isValid = isValid && api.Validate();
            }

            return isValid;
        }
    }
}
