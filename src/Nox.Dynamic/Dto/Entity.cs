using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    public class Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string Table { get; set; } = null!;
        public string Schema { get; set; } = "dbo";
        public List<string> RelatedParents { get; set; } = Enumerable.Empty<string>().ToList();
        public List<EntityAttribute> Attributes { get; set; } = Enumerable.Empty<EntityAttribute>().ToList();

    }
}
