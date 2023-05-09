using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class ApiRouteParameterConfigValidator: AbstractValidator<ApiRouteParameterConfiguration>
{
    public ApiRouteParameterConfigValidator()
    {
        RuleFor(rp => rp.Name)
            .NotEmpty()
            .WithMessage(rp => string.Format(ValidationResources.ApiRouteParameterNameEmpty, rp.DefinitionFileName));
        
        RuleFor(rp => rp.Type)
            .NotEmpty()
            .WithMessage(rp => string.Format(ValidationResources.ApiRouteParameterTypeEmpty, rp.Name, rp.DefinitionFileName));
    }
}