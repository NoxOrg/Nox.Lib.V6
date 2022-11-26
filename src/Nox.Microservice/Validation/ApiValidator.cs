using FluentValidation;
using Nox.Core.Models;

namespace Nox.Microservice.Validation;

public class ApiValidator : AbstractValidator<Core.Models.Api>
{
    public ApiValidator()
    {

        RuleFor( api => api.Name)
            .NotEmpty()
            .WithMessage(api => $"The api's name must be specified in {api.DefinitionFileName}");

    }
}