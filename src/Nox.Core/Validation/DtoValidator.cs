using FluentValidation;
using Nox.Core.Interfaces.Dto;

namespace Nox.Core.Validation;

public class DtoValidator: AbstractValidator<IDto>
{
    public DtoValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage(entity => $"The DTO's name must be specified in {entity.DefinitionFileName}");

        RuleFor(dto => dto.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(entity => $"Defaults could not be applied to DTO defined in {entity.DefinitionFileName}");

        RuleFor(dto => dto.Attributes)
            .NotEmpty()
            .WithMessage(dto => $"The DTO must have at least one property defined in {dto.DefinitionFileName}");

        RuleForEach(dto => dto.Attributes)
            .SetValidator(new DtoAttributeValidator());
    }
}