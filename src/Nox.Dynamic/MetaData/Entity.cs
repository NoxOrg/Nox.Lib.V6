using Humanizer;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nox.Dynamic.MetaData
{
    public sealed class Entity : MetaBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string Table { get; set; } = null!;
        public string Schema { get; set; } = "dbo";
        [NotMapped]
        public string[] RelatedParents { get; set; } = Array.Empty<string>();
        public string RelatedParentsJson { get => string.Join('|',RelatedParents.ToArray()); set => RelatedParents = value.Split('|'); }
        public ICollection<EntityAttribute> Attributes { get; set; } = new Collection<EntityAttribute>();


        public bool Validate()
        {
            var isValid = true;

            // Validation - should throw pretty exception

            if (string.IsNullOrWhiteSpace(Name)) return false;

            if (Attributes.Count == 0) return false;

            // Defaults

            if (string.IsNullOrWhiteSpace(PluralName))
                PluralName = Name.Pluralize();

            if (string.IsNullOrWhiteSpace(Table))
                Table = Name;

            if (string.IsNullOrWhiteSpace(Schema))
                Schema = "dbo";

            // validate contained definitions

            foreach (var attribute in Attributes)
            {
                isValid = isValid && attribute.Validate();
            }

            return isValid;
        }


    }
}
