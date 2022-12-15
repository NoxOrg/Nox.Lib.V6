using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityAttributeConfigValidator: AbstractValidator<EntityAttributeConfiguration>
{
    public EntityAttributeConfigValidator()
    {
        RuleFor(attribute => attribute.Name)
            .NotEmpty()
            .WithMessage(attribute => string.Format(ValidationResources.EntityAttributeNameEmpty, attribute.DefinitionFileName));

        RuleFor(attribute => attribute.Type)
            .NotEmpty()
            .WithMessage(attribute => string.Format(ValidationResources.EntityAttributeTypeEmpty, attribute.Name, attribute.DefinitionFileName));
    }
}