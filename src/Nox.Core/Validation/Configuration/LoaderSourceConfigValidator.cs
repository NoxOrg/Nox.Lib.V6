using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderSourceConfigValidator: AbstractValidator<LoaderSourceConfiguration>
{
    public LoaderSourceConfigValidator(List<DataSourceConfiguration>? dataSources)
    {
        RuleFor( ls => ls.DataSource)
            .NotEmpty()
            .WithMessage(ls => string.Format(ValidationResources.LoaderDataSourceEmpty, ls.DefinitionFileName));
        
        RuleFor( ls => ls.Query)
            .NotEmpty()
            .WithMessage(ls => string.Format(ValidationResources.LoaderQueryEmpty, ls.DefinitionFileName));
        
        RuleFor(ls => ls!.DataSource)
            .Must(dsName => dataSources != null && dataSources.Exists(ec => ec.Name == dsName))
            .WithMessage(ls => string.Format(ValidationResources.LoaderDataSourceMissing, ls.DataSource, ls.DefinitionFileName));
    }
}