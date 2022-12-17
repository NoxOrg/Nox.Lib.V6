using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderScheduleConfigValidator: AbstractValidator<LoaderScheduleConfiguration?>
{
    public LoaderScheduleConfigValidator()
    {
        RuleFor(ls => ls!.Start)
            .NotEmpty()
            .WithMessage(ls => string.Format(ValidationResources.LoaderScheduleStartEmpty, ls!.DefinitionFileName));
    }
}