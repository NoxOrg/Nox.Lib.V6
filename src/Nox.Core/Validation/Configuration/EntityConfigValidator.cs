using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityConfigValidator: AbstractValidator<EntityConfiguration>
{
    public EntityConfigValidator()
    {
        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => $"The entity's name must be specified in {entity.DefinitionFileName}");

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => $"The entity must have at least one property defined in {entity.DefinitionFileName}");

        RuleForEach(entity => entity.Attributes)
            .SetValidator(new EntityAttributeConfigValidator());
    }
}