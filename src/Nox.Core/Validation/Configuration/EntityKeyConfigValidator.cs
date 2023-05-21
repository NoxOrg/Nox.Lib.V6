using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityKeyConfigValidator : AbstractValidator<EntityKeyConfiguration>
{
    public EntityKeyConfigValidator()
    {
        RuleFor(key => key.Name)
            .NotEmpty()
            .When(k => !k.Entities.Any())
            .WithMessage(key => string.Format(ValidationResources.EntityKeyNameEmpty, key.DefinitionFileName));

        RuleFor(key => key.Type)
            .NotEmpty()
            .When(k => !k.Entities.Any())
            .WithMessage(key => string.Format(ValidationResources.EntityKeyTypeEmpty, key.Name, key.DefinitionFileName));
    }
}