using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class MessagingProviderConfigValidator: AbstractValidator<MessagingProviderConfiguration>
{
    public MessagingProviderConfigValidator()
    {
        RuleFor( mp => mp.Name)
            .NotEmpty()
            .WithMessage(mp => string.Format(ValidationResources.MessagingProviderNameEmpty, mp.DefinitionFileName));
        
        RuleFor( mp => mp.Provider)
            .NotEmpty()
            .WithMessage(mp => string.Format(ValidationResources.MessagingProviderProviderEmpty, mp.Name, mp.DefinitionFileName));
    }
}