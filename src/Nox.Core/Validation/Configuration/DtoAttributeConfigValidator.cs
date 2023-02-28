using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class DtoAttributeConfigValidator: AbstractValidator<DtoAttributeConfiguration>
{
    public DtoAttributeConfigValidator()
    {
        RuleFor(attribute => attribute.Name)
            .NotEmpty()
            .WithMessage(attribute => string.Format(ValidationResources.DtoAttributeNameEmpty, attribute.DefinitionFileName));

        RuleFor(attribute => attribute.Type)
            .NotEmpty()
            .WithMessage(attribute => string.Format(ValidationResources.DtoAttributeTypeEmpty, attribute.Name, attribute.DefinitionFileName));
    }
}