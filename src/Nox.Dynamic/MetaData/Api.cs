using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
{
    public sealed class Api : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ApiRoute> Routes { get; set; } = new Collection<ApiRoute>();

        public bool Validate()
        {
            var isValid = true;

            // Validation - should throw pretty exception

            if (string.IsNullOrWhiteSpace(Name)) return false;

            return isValid;
        }

    }
}
