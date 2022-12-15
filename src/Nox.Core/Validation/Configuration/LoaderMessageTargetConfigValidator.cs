using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderMessageTargetConfigValidator: AbstractValidator<LoaderMessageTargetConfiguration>
{
    public LoaderMessageTargetConfigValidator()
    {
        RuleFor(lmt => lmt.MessagingProvider)
            .NotEmpty()
            .WithMessage(lmt => string.Format(ValidationResources.LoaderMessageTargetProviderEmpty, lmt.DefinitionFileName));
    }
}