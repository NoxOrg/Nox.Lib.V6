using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class ApiConfigValidator: AbstractValidator<ApiConfiguration>
{
    public ApiConfigValidator()
    {
        RuleFor( api => api.Name)
            .NotEmpty()
            .WithMessage(api => string.Format(ValidationResources.ApiNameEmpty, api.DefinitionFileName));
        
        RuleFor(api => api.Routes)
            .NotEmpty()
            .WithMessage(api => string.Format(ValidationResources.ApiRoutesEmpty, api.DefinitionFileName));
        
        RuleForEach(api => api.Routes)
            .SetValidator(new ApiRouteConfigValidator());
    }
}