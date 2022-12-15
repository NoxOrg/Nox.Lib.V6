using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderTargetConfigValidator: AbstractValidator<LoaderTargetConfiguration?>
{
    public LoaderTargetConfigValidator()
    {
        RuleFor(lt => lt!.Entity)
            .NotEmpty()
            .WithMessage(lt => string.Format(ValidationResources.LoaderTargetEntityEmpty, lt!.DefinitionFileName));
//        RuleFor(lt => lt!.Entity)
            
    }
    
}