using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public List<string> RelatedParents { get; set; } = Enumerable.Empty<string>().ToList();
        public List<Property> Properties { get; set; } = Enumerable.Empty<Property>().ToList();
    }
}
