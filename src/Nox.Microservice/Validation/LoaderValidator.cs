using FluentValidation;
using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Models;

namespace Nox.Microservice.Validation;

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