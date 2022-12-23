using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityConfigValidator: AbstractValidator<EntityConfiguration>
{
    public EntityConfigValidator(List<MessagingProviderConfiguration>? msgProviders)
    {
        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => string.Format(ValidationResources.EntityNameEmpty, entity.DefinitionFileName));

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => string.Format(ValidationResources.EntityAttributesEmpty, entity.Name, entity.DefinitionFileName));

        RuleForEach(entity => entity.Attributes)
            .SetValidator(new EntityAttributeConfigValidator());
        
        RuleForEach(entity => entity.Messaging)
            .SetValidator(new MessageTargetConfigValidator(msgProviders));
    }
}