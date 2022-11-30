using FluentValidation;

namespace Nox.Microservice.Validation;

public class ApiValidator : AbstractValidator<Api.Api>
{
    public ApiValidator()
    {

        RuleFor( api => api.Name)
            .NotEmpty()
            .WithMessage(api => $"The api's name must be specified in {api.DefinitionFileName}");

    }
}