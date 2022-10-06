using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData
{
    public sealed class Loader : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LoaderSchedule Schedule { get; set; } = new();
        public LoaderLoadStrategy LoadStrategy { get; set; } = new();
        public LoaderTarget Target { get; set; } = new();
        public ICollection<LoaderSource> Sources { get; set; } = new Collection<LoaderSource>();

        public bool Validate()
        {
            var isValid = true;

            // Validation - should throw pretty exception

            if (string.IsNullOrWhiteSpace(Name)) return false;

            return isValid;
        }

    }

}
