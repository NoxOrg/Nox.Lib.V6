using System.Collections.Generic;

namespace Nox.Generator.Generators.Entities
{
    internal class EntityWithCompositeKey
    {
        public EntityWithCompositeKey(string entityName) 
        {
            EntityName = entityName;
        }

        public string EntityName { get; set; }

        public List<string> KeyEntities { get; set; } = new List<string>();
    }
}