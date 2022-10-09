using FluentValidation;
using Humanizer;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nox.Dynamic.MetaData;

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

    public bool ApplyDefaults()
    {
        if (string.IsNullOrWhiteSpace(PluralName))
            PluralName = Name.Pluralize();

        if (string.IsNullOrWhiteSpace(Table))
            Table = Name;

        if (string.IsNullOrWhiteSpace(Schema))
            Schema = "dbo";

        return true;
    }
}

internal class EntityValidator : AbstractValidator<Entity>
{
    public EntityValidator(ServiceValidationInfo info)
    {

        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => $"[{info.ServiceName}] The entity's name must be specified in {entity.DefinitionFileName}");

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => $"[{info.ServiceName}] The entity must have at least one property defined in {entity.DefinitionFileName}");

        RuleForEach(entity => entity.Attributes)
            .SetValidator(new EntityAttributeValidator(info));

        RuleFor(entity => entity.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(entity => $"[{info.ServiceName}] Defaults could not be applied to entity defined in {entity.DefinitionFileName}");

    }
}

