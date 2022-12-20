using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderConfigValidator: AbstractValidator<LoaderConfiguration>
{
    public LoaderConfigValidator(List<EntityConfiguration>? entities, List<DataSourceConfiguration>? dataSources, List<MessagingProviderConfiguration>? msgProviders)
    {
        RuleFor( loader => loader.Name)
            .NotEmpty()
            .WithMessage(loader => string.Format(ValidationResources.LoaderNameEmpty, loader.DefinitionFileName));

        RuleFor(loader => loader.LoadStrategy)
            .NotEmpty()
            .WithMessage(loader => string.Format(ValidationResources.LoaderLoadStrategyEmpty, loader.Name, loader.DefinitionFileName));
        
        RuleFor(loader => loader.Schedule)
            .SetValidator(new LoaderScheduleConfigValidator());

        
        RuleFor(loader => loader.LoadStrategy)
            .SetValidator(new LoaderLoadStrategyConfigValidator());

        RuleFor(loader => loader.Target)
            .NotEmpty()
            .WithMessage(loader => string.Format(ValidationResources.LoaderTargetEmpty, loader.Name, loader.DefinitionFileName));

        RuleFor(loader => loader.Target)
            .SetValidator(_ => new LoaderTargetConfigValidator(entities));
        
        RuleForEach(loader => loader.Messaging)
            .SetValidator(new LoaderMessageTargetConfigValidator(msgProviders));
        
        RuleForEach(loader => loader.Sources)
            .SetValidator(new LoaderSourceConfigValidator(dataSources));
        
        RuleForEach(loader => loader.Messaging)
            .SetValidator(new LoaderMessageTargetConfigValidator(msgProviders));
    }
}