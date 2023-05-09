using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

internal class YamlConfigValidator: AbstractValidator<YamlConfiguration>
{
    public YamlConfigValidator()
    {
        RuleFor(config => config.Name)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigNameEmpty, config.DefinitionFileName));

        RuleFor(config => config.Database)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigDbEmpty, config.DefinitionFileName));

        RuleFor(config => config.Database!)
            .SetValidator(config => new DataSourceConfigValidator(true));

        RuleForEach(config => config.MessagingProviders)
            .SetValidator(new MessagingProviderConfigValidator());
        
        RuleForEach(config => config.Apis)
            .SetValidator(new ApiConfigValidator());

        /* We should be able to define services without Entities: Andre - test this..
         
        RuleFor(config => config.Entities)
            .NotEmpty()
            .WithMessage(config => string.Format(ValidationResources.ConfigEntitiesEmpty, config.Name, config.DefinitionFileName));
        */

        RuleForEach(config => config.Entities)
            .SetValidator(config => new EntityConfigValidator(config.MessagingProviders));
        
        RuleForEach(config => config.Loaders)
            .SetValidator(config => new LoaderConfigValidator(config.Entities, config.DataSources, config.MessagingProviders));
        
        RuleForEach(config => config.DataSources)
            .SetValidator(config => new DataSourceConfigValidator(false));
    }
}