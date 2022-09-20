using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class EntityDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PluralName { get; set; }
        public string? Table { get; set; }
        public string? Schema { get; set; }
        public List<string> RelatedParents { get; set; } = new();
        public List<PropertyDefinition> Properties { get; set; } = new();
    }
}
