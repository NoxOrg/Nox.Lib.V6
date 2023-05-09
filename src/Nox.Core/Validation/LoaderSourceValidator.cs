using FluentValidation;
using Nox.Core.Interfaces.Etl;

namespace Nox.Core.Validation;

public class LoaderSourceValidator : AbstractValidator<ILoaderSource>
{
    public LoaderSourceValidator()
    {
        RuleFor(ls => ls.DataSource)
            .NotEmpty()
            .WithMessage(ls => $"Loader Source defined in {ls.DefinitionFileName} must have a Name.");
    }
}