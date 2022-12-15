using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderSourceConfigValidator: AbstractValidator<LoaderSourceConfiguration>
{
    public LoaderSourceConfigValidator()
    {
        RuleFor( ls => ls.DataSource)
            .NotEmpty()
            .WithMessage(ls => string.Format(ValidationResources.LoaderDataSourceEmpty, ls.DefinitionFileName));
        
        RuleFor( ls => ls.Query)
            .NotEmpty()
            .WithMessage(ls => string.Format(ValidationResources.LoaderQueryEmpty, ls.DefinitionFileName));
    }
}