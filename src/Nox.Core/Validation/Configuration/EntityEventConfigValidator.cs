using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityEventConfigValidator : AbstractValidator<EventConfiguration>
{
    public EntityEventConfigValidator()
    {
        RuleFor(key => key.Name)
            .NotEmpty()
            .WithMessage(key => string.Format(ValidationResources.EntityDomainEventNameEmpty, key.DefinitionFileName));

        RuleFor(key => key.Type)
            .NotEmpty()
            .WithMessage(key => string.Format(ValidationResources.EntityDomainEventTypeEmpty, key.DefinitionFileName, key.Name));
    }
}