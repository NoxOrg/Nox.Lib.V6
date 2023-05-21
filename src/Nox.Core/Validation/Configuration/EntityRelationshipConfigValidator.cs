using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityRelationshipConfigValidator : AbstractValidator<EntityRelationshipConfiguration>
{
    public EntityRelationshipConfigValidator()
    {
        RuleFor(relationship => relationship.Name)
            .NotEmpty()
            .WithMessage(relationship => string.Format(ValidationResources.EntityRelationshipNameEmpty, relationship.DefinitionFileName));

        RuleFor(relationship => relationship.Entity)
            .NotEmpty()
            .WithMessage(relationship => string.Format(ValidationResources.EntityRelationshipEntityEmpty, relationship.Name, relationship.DefinitionFileName));
    }
}