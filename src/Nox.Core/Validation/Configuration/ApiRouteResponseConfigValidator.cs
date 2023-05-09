using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class ApiRouteResponseConfigValidator: AbstractValidator<ApiRouteResponseConfiguration>
{
    public ApiRouteResponseConfigValidator()
    {
        RuleFor(rr => rr.Type)
            .NotEmpty()
            .WithMessage(rr => string.Format(ValidationResources.ApiRouteResponseTypeEmpty, rr.DefinitionFileName));
    }
}