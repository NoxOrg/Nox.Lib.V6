using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class Loader
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LoaderSchedule Schedule { get; set; } = new();
        public LoaderLoadStrategy LoadStrategy { get; set; } = new();
        public LoaderTarget Target { get; set; } = new();
        public List<LoaderSource> Sources { get; set; } = new();
    }
}
