using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class ApiRouteConfigValidator: AbstractValidator<ApiRouteConfiguration>
{
    public ApiRouteConfigValidator()
    {
        RuleFor( route => route.Name)
            .NotEmpty()
            .WithMessage(route => string.Format(ValidationResources.ApiRouteNameEmpty, route.DefinitionFileName));
        
        RuleFor( route => route.HttpVerb)
            .NotEmpty()
            .WithMessage(route => string.Format(ValidationResources.ApiRouteVerbEmpty, route.DefinitionFileName));
        
        RuleFor( route => route.TargetUrl)
            .NotEmpty()
            .WithMessage(route => string.Format(ValidationResources.ApiRouteTargetUrlEmpty, route.DefinitionFileName));
        
        RuleForEach(route => route.Parameters)
            .SetValidator(new ApiRouteParameterConfigValidator());
        
        RuleForEach(route => route.Responses)
            .SetValidator(new ApiRouteResponseConfigValidator());
    }
}