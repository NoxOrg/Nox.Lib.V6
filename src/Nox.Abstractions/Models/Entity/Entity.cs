using FluentValidation;
using Humanizer;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nox;

public sealed class Entity : ModelBase, IEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public string Table { get; set; } = null!;
    public string Schema { get; set; } = "dbo";
    [NotMapped]
    public List<string> RelatedParents { get; set; } = new();
    [NotMapped]
    public List<string> RelatedChildren { get; set; } = new();
    public string RelatedParentsJson { get => string.Join('|',RelatedParents.ToArray()); set => RelatedParents = value.Split('|').ToList(); }
    public string RelatedChildrenJson { get => string.Join('|',RelatedChildren.ToArray()); set => RelatedChildren = value.Split('|').ToList(); }
    public int SortOrder { get; set; }
    public ICollection<EntityAttribute> Attributes { get; set; } = new Collection<EntityAttribute>();

    public bool ApplyDefaults()
    {
        if (string.IsNullOrWhiteSpace(PluralName))
            PluralName = Name.Pluralize();

        if (string.IsNullOrWhiteSpace(Table))
            Table = Name;

        if (string.IsNullOrWhiteSpace(Schema))
            Schema = "dbo";

        RelatedChildren = RelatedChildren.Where(x => x.Trim().Length > 0).ToList();

        RelatedParents = RelatedParents.Where(x => x.Trim().Length > 0).ToList();

        return true;
    }
}

public class EntityValidator : AbstractValidator<IEntity>
{
    public EntityValidator()
    {

        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => $"The entity's name must be specified in {entity.DefinitionFileName}");

        RuleFor(entity => entity.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(entity => $"Defaults could not be applied to entity defined in {entity.DefinitionFileName}");

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => $"The entity must have at least one property defined in {entity.DefinitionFileName}");

        RuleForEach(entity => entity.Attributes)
            .SetValidator(new EntityAttributeValidator());

    }
}

