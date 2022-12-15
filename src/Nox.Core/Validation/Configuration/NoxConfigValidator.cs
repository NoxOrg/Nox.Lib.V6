using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class NoxConfigValidator: AbstractValidator<NoxConfiguration>
{
    public NoxConfigValidator()
    {
        RuleFor(config => config.Name)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigNameEmpty, config.DefinitionFileName));

        RuleFor(config => config.Database)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigDbEmpty, config.DefinitionFileName));

        RuleForEach(config => config.MessagingProviders)
            .SetValidator(new MessagingProviderConfigValidator());
        
        RuleForEach(config => config.Apis)
            .SetValidator(new ApiConfigValidator());

        RuleFor(config => config.Entities)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigEntitiesEmpty, config.Name, config.DefinitionFileName));

        RuleForEach(config => config.Entities)
            .SetValidator(new EntityConfigValidator());
        
        RuleForEach(config => config.Loaders)
            .SetValidator(new LoaderConfigValidator());
        
        RuleForEach(config => config.DataSources)
            .SetValidator(new DatabaseConfigValidator());
    }
}