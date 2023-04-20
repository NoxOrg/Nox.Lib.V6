using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityCommandConfigValidator : AbstractValidator<CommandConfiguration>
{
    public EntityCommandConfigValidator()
    {
        RuleFor(key => key.Name)
            .NotEmpty()
            .WithMessage(key => string.Format(ValidationResources.EntityCommandNameEmpty, key.DefinitionFileName));
    }
}