using FluentValidation;
using Nox.Core.Interfaces;

namespace Nox.Lib.Validation;

public class LoaderValidator : AbstractValidator<ILoader>
{
    public LoaderValidator()
    {
        RuleFor(loader => loader.Name)
            .NotEmpty()
            .WithMessage(loader => $"The data loader name must be specified in {loader.DefinitionFileName}");

        RuleFor(loader => loader.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(loader => $"Defaults could not be applied to loader defined in {loader.DefinitionFileName}");

        RuleForEach(loader => loader.Sources)
            .SetValidator(new LoaderSourceValidator());

    }
}