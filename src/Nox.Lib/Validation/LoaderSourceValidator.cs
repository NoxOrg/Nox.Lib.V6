using FluentValidation;
using Nox.Core.Interfaces;

namespace Nox.Lib.Validation;

public class LoaderSourceValidator : AbstractValidator<ILoaderSource>
{
    public LoaderSourceValidator()
    {
        RuleFor(ls => ls.Name)
            .NotEmpty()
            .WithMessage(ls => $"Loader Source defined in {ls.DefinitionFileName} must have a Name.");
    }
}