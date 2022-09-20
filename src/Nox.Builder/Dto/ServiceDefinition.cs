using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class ServiceDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ServiceDatabase? Database { get; set; }
    }

    internal class ServiceDatabase
    {
        public string Name { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public string? Server { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Options { get; set; }
        public string? ConnectionString { get; set; }
    }
}
