using FluentValidation;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Validation;

public class EntityKeyValidator : AbstractValidator<IEntityKey>
{
    public EntityKeyValidator()
    {
        RuleFor(attribute => attribute.Entities)
            .NotEmpty()
            .NotNull()
            .When(k => k.IsComposite)
            .WithMessage(attribute => $"The key name must be specified in {attribute.DefinitionFileName}");

        RuleFor(attribute => attribute.Name)
            .NotEmpty()
            .When(k => !k.IsComposite)
            .WithMessage(attribute => $"The key name must be specified in {attribute.DefinitionFileName}");

        RuleFor(attribute => attribute.Type)
            .NotEmpty()
            .When(k => !k.IsComposite)
            .WithMessage(attribute => $"Entity key '{attribute.Name}' has no type defined in {attribute.DefinitionFileName}");
    }
}