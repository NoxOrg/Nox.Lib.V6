using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class DtoConfigValidator: AbstractValidator<DtoConfiguration>
{
    public DtoConfigValidator()
    {
        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => string.Format(ValidationResources.DtoNameEmpty, entity.DefinitionFileName));

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => string.Format(ValidationResources.DtoAttributesEmpty, entity.Name, entity.DefinitionFileName));

        RuleForEach(dto => dto.Attributes)
            .SetValidator(new DtoAttributeConfigValidator());
    }
}