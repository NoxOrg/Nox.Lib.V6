using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityQueryParameterConfigValidator : AbstractValidator<ParameterConfiguration>
{
    public EntityQueryParameterConfigValidator()
    {
        RuleFor(parameter => parameter.Name)
            .NotEmpty()
            .WithMessage(parameter => string.Format(ValidationResources.EntityQueryParameterNameEmpty, parameter.DefinitionFileName));

        RuleFor(parameter => parameter.Type)
            .NotEmpty()
            .WithMessage(parameter => string.Format(ValidationResources.EntityQueryParameterTypeEmpty, parameter.Name, parameter.DefinitionFileName));
    }
}