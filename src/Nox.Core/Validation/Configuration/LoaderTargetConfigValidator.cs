using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderTargetConfigValidator: AbstractValidator<LoaderTargetConfiguration?>
{
    public LoaderTargetConfigValidator(List<EntityConfiguration>? entityConfig)
    {
        RuleFor(lt => lt!.Entity)
            .NotEmpty()
            .WithMessage(lt => string.Format(ValidationResources.LoaderTargetEntityEmpty, lt!.DefinitionFileName));
        RuleFor(lt => lt!.Entity)
            .Must(entityName => entityConfig != null && entityConfig.Exists(ec => ec.Name == entityName))
            .WithMessage(lt => string.Format(ValidationResources.LoaderTargetEntityMissing, lt.Entity, lt.DefinitionFileName));

    }
    
}